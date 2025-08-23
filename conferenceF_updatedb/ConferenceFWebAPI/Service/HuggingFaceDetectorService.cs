using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using ConferenceFWebAPI.DTOs.AICheckDTO;

namespace ConferenceFWebAPI.Service
{
    public class HuggingFaceDetectorService : IAiDetectorService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly string _model = "MonkeyDAnh/my-awesome-ai-detector-roberta-base-v4-human-vs-machine-finetune";

        private const int MaxTokens = 512; // Giới hạn tối đa 512 token
        private const int OverlapTokens = 128; // Overlap 128 token
        private const int MaxWordsPerChunk = (int)(MaxTokens / 100.0 * 75); // ~384 từ (512 token)
        private const int OverlapWords = (int)(OverlapTokens / 100.0 * 75); // ~96 từ (128 token)
        private const int MaxTextLength = 50000; // Giới hạn tối đa 50,000 ký tự (~20 trang)

        public HuggingFaceDetectorService(IHttpClientFactory httpFactory, IMemoryCache cache, IConfiguration config)
        {
            _http = httpFactory.CreateClient();
            _cache = cache;

            var token = config["HF_API_TOKEN"] ?? throw new InvalidOperationException("HF_API_TOKEN missing");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("ConferenceFWebAPI/1.0");

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Giữ nguyên xuống dòng và khoảng trắng, chỉ loại bỏ ký tự không mong muốn nếu cần.
        /// </summary>
        private static string PrepareText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text
                .Replace("\r\n", "\n") // Chuẩn hóa xuống dòng
                .Replace("\r", "\n") // Đảm bảo giữ nguyên định dạng
                .Replace("\u22A4", "transpose")
                .Replace("[^\\w\\s.,!?()'-=]", "");
        }

        /// <summary>
        /// Chunk theo số từ, có overlap, giữ nguyên định dạng, đảm bảo mỗi chunk dưới 512 token (~384 từ).
        /// </summary>
        private static List<string> SplitTextWithOverlap(string text, int maxWords, int overlapWords)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            var words = text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            for (int start = 0; start < words.Length; start += (maxWords - overlapWords))
            {
                var remainingWords = words.Length - start;
                var length = Math.Min(maxWords, remainingWords);
                if (length <= 0) break;

                var chunkWords = words.Skip(start).Take(length).ToArray();
                var chunk = string.Join(" ", chunkWords).Trim();
                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                if (start + length >= words.Length) break;
            }

            return chunks.Any() ? chunks : new List<string> { text }; // Fallback nếu không chunk
        }

        public async Task<AnalyzeAiResponseDTO> AnalyzeTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                throw new ArgumentException("Raw text is required.");

            if (rawText.Length > MaxTextLength)
                throw new ArgumentException($"Text exceeds maximum length of {MaxTextLength} characters.");

            var text = PrepareText(rawText);
            Console.WriteLine($"Raw text length: {rawText.Length}, Prepared text length: {text.Length}, Word count: {text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length}, Content: {text.Substring(0, Math.Min(100, text.Length))}...");

            var key = ComputeHash(_model + "|" + text);
            var cacheKey = $"hf_full_{key}";
            if (_cache.TryGetValue(cacheKey, out AnalyzeAiResponseDTO cached))
                return cached;

            var splitChunks = SplitTextWithOverlap(text, MaxWordsPerChunk, OverlapWords);
            Console.WriteLine($"Number of chunks created: {splitChunks.Count}, First chunk: {splitChunks.FirstOrDefault()?.Substring(0, Math.Min(100, splitChunks.FirstOrDefault()?.Length ?? 0))}...");

            var chunkResults = new List<ChunkResultDTO>();
            double sumAiPercent = 0;
            int ok = 0;
            int chunkId = 1;
            int totalTokens = 0;

            foreach (var ch in splitChunks)
            {
                var prob = await InferMachineProbabilityAsync(ch);
                Console.WriteLine($"Chunk {chunkId} word count: {ch.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length}, Probability: {prob}");
                if (prob.HasValue)
                {
                    sumAiPercent += prob.Value * 100.0;
                    ok++;
                    var wordCount = ch.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    var estimatedTokens = (int)(wordCount / 75.0 * 100); // Chuyển từ số từ sang số token
                    totalTokens += estimatedTokens;
                    chunkResults.Add(new ChunkResultDTO
                    {
                        ChunkId = chunkId++,
                        TokenCount = estimatedTokens,
                        ScoreMachine = Math.Round(prob.Value * 100.0, 2),
                        ScoreHuman = Math.Round(100 - prob.Value * 100.0, 2)
                    });
                }
            }

            var aiPercent = ok > 0 ? sumAiPercent / ok : 0.0;
            var aiTokenEquiv = totalTokens * (aiPercent / 100.0);
            Console.WriteLine($"ok: {ok}, aiPercent: {aiPercent}, totalTokens: {totalTokens}");

            var result = new AnalyzeAiResponseDTO
            {
                PercentAi = Math.Round(aiPercent, 2),
                TotalTokens = totalTokens,
                AiTokenEquiv = Math.Round(aiTokenEquiv, 2),
                Chunks = chunkResults
            };

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }

        public async Task<ChunkResultDTO?> AnalyzeChunkAsync(ChunkPayloadDTO chunk)
        {
            return null; // Không dùng nữa
        }

        private async Task<double?> InferMachineProbabilityAsync(string text)
        {
            var url = $"https://api-inference.huggingface.co/models/{_model}";
            var payload = new
            {
                inputs = text,
                parameters = new { truncation = true, max_length = 512 }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _retryPolicy.ExecuteAsync(() => _http.PostAsync(url, content));
            Console.WriteLine($"HF API response status: {response.StatusCode}, Body: {await response.Content.ReadAsStringAsync()}");

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();

            try
            {
                var arr = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(body);
                if (arr != null && arr.Count > 0)
                {
                    var machine = arr.FirstOrDefault(d =>
                                d.TryGetValue("label", out var l) &&
                                l?.ToString()?.ToLower().Contains("machine") == true)
                            ?? arr.OrderByDescending(d => Convert.ToDouble(d["score"])).FirstOrDefault();

                    if (machine != null && machine.TryGetValue("score", out var sc))
                        return Convert.ToDouble(sc);
                }
                Console.WriteLine("No valid score found in HF response.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing HF response: {ex.Message}");
                return null;
            }
            return null;
        }
    }
}