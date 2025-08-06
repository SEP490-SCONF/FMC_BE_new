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

        public HuggingFaceDetectorService(IHttpClientFactory httpFactory, IMemoryCache cache, IConfiguration config)
        {
            _http = httpFactory.CreateClient();
            _cache = cache;

            var token = config["HF_API_TOKEN"] ?? throw new InvalidOperationException("HF_API_TOKEN missing");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("ConferenceFWebAPI/1.0");

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => ((int)r.StatusCode >= 500) || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }

        private string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        public async Task<ChunkResultDTO?> AnalyzeChunkAsync(ChunkPayloadDTO chunk)
        {
            var key = chunk.Hash ?? ComputeHash(chunk.Text);
            var cacheKey = $"hf_chunk_{key}";
            if (_cache.TryGetValue<ChunkResultDTO>(cacheKey, out var cached))
            {
                return cached;
            }

            var payload = new { inputs = chunk.Text };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://api-inference.huggingface.co/models/{_model}";

            var response = await _retryPolicy.ExecuteAsync(() => _http.PostAsync(url, content));

            if (!response.IsSuccessStatusCode)
            {
                return new ChunkResultDTO
                {
                    ChunkId = chunk.ChunkId,
                    TokenCount = chunk.TokenCount,
                    ScoreMachine = 0,
                    ScoreHuman = 1
                };
            }

            var body = await response.Content.ReadAsStringAsync();
            List<Dictionary<string, object>>? arr = null;
            try
            {
                arr = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(body);
            }
            catch { }

            double scoreMachine = 0;
            if (arr != null)
            {
                var machineEntry = arr.FirstOrDefault(d =>
                    d.TryGetValue("label", out var l) &&
                    l.ToString()?.ToLower().Contains("machine") == true);

                if (machineEntry != null && machineEntry.TryGetValue("score", out var sc))
                {
                    scoreMachine = Convert.ToDouble(sc);
                }
            }

            var result = new ChunkResultDTO
            {
                ChunkId = chunk.ChunkId,
                TokenCount = chunk.TokenCount,
                ScoreMachine = Math.Round(scoreMachine, 4),
                ScoreHuman = Math.Round(1 - scoreMachine, 4)
            };

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
    }
}
