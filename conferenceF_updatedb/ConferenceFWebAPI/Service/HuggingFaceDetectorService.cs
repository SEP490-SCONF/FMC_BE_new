using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ConferenceFWebAPI.DTOs.AICheckDTO;
using System.Security.Cryptography;
using System;
using Tokenizers.DotNet;

namespace ConferenceFWebAPI.Service
{
    public class HuggingFaceDetectorService : IAiDetectorService
    {
        private readonly IMemoryCache _cache;
        private readonly InferenceSession _session;
        private readonly Tokenizer _tokenizer;

        private const int MaxTokens = 512;     // giới hạn tối đa 512 token
        private const int OverlapTokens = 128; // overlap
        private const int MaxTextLength = 50000;

        // chúng ta tự quản lý vocab để lookup id cho special tokens
        private readonly Dictionary<string, int> _vocab = new();
        private readonly int _padTokenId = 0;

        public HuggingFaceDetectorService(IMemoryCache cache)
        {
            _cache = cache;

            // Load ONNX model
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "model.onnx");
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Model file not found at " + modelPath);
            _session = new InferenceSession(modelPath);

            // Đường dẫn tokenizer & vocab
            var modelsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            var tokenizerPath = Path.Combine(modelsDir, "tokenizer.json");
            var vocabPath = Path.Combine(modelsDir, "vocab.json");
            var specialPath = Path.Combine(modelsDir, "special_tokens_map.json");

            if (!File.Exists(tokenizerPath))
                throw new FileNotFoundException("Tokenizer file not found at " + tokenizerPath);

            // Khởi tạo Tokenizers.DotNet bằng tokenizer.json
            _tokenizer = new Tokenizer(vocabPath: tokenizerPath);

            // Đọc vocab.json (nếu có) để map token -> id
            if (File.Exists(vocabPath))
            {
                var vocabJson = File.ReadAllText(vocabPath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, int>>(vocabJson);
                if (dict != null)
                {
                    _vocab = dict;
                }
            }

            // Xác định pad_token_id theo thứ tự ưu tiên:
            // 1) special_tokens_map.json -> "pad_token" -> tra id trong vocab.json
            // 2) nếu không có, thử các biến thể phổ biến
            // 3) fallback về 0
            int padId = 0;
            if (File.Exists(specialPath))
            {
                try
                {
                    var specialJson = File.ReadAllText(specialPath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(specialJson);
                    if (dict != null && dict.TryGetValue("pad_token", out var padObj))
                    {
                        var padToken = padObj?.ToString();
                        if (!string.IsNullOrEmpty(padToken) && _vocab.TryGetValue(padToken!, out var vid))
                        {
                            padId = vid;
                        }
                    }
                }
                catch { /* bỏ qua, dùng fallback */ }
            }
            if (padId == 0 && _vocab.Count > 0)
            {
                if (_vocab.TryGetValue("[PAD]", out var vidPad)) padId = vidPad;
                else if (_vocab.TryGetValue("<pad>", out var vidPad2)) padId = vidPad2;
                else if (_vocab.TryGetValue("<|pad|>", out var vidPad3)) padId = vidPad3;
            }
            _padTokenId = padId; // nếu vẫn 0 thì coi như model dùng 0 làm PAD
        }

        /// <summary>
        /// Encode text thành nhiều chunk với overlap (dùng Tokenizers.DotNet → uint[])
        /// </summary>
        private List<(uint[] inputIds, int[] attentionMask, int realCount)> EncodeWithOverflowing(
            string text, int maxLength = 512, int stride = 128)
        {
            if (maxLength <= 0) throw new ArgumentException("maxLength must be > 0");
            if (stride < 0 || stride >= maxLength) stride = Math.Max(0, maxLength / 4); // guard

            var ids = _tokenizer.Encode(text); // uint[]
            var chunks = new List<(uint[], int[], int)>();
            var step = Math.Max(1, maxLength - stride);

            for (int start = 0; start < ids.Length; start += step)
            {
                var end = Math.Min(start + maxLength, ids.Length);
                var sub = ids.Skip(start).Take(end - start).ToList();
                int realCount = sub.Count;

                // pad nếu chưa đủ
                if (sub.Count < maxLength)
                    sub.AddRange(Enumerable.Repeat((uint)_padTokenId, maxLength - sub.Count));

                var attentionMask = sub.Select((_, i) => i < realCount ? 1 : 0).ToArray();
                chunks.Add((sub.ToArray(), attentionMask, realCount));

                if (end >= ids.Length) break;
            }

            // Trường hợp text quá ngắn (< maxLength), vẫn tạo 1 chunk duy nhất đã pad
            if (chunks.Count == 0)
            {
                var sub = ids.ToList();
                int realCount = sub.Count;
                if (sub.Count < maxLength)
                    sub.AddRange(Enumerable.Repeat((uint)_padTokenId, maxLength - sub.Count));
                var attentionMask = sub.Select((_, i) => i < realCount ? 1 : 0).ToArray();
                chunks.Add((sub.ToArray(), attentionMask, realCount));
            }

            return chunks;
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        private static string PrepareText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        public async Task<AnalyzeAiResponseDTO> AnalyzeTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                throw new ArgumentException("Raw text is required.");
            if (rawText.Length > MaxTextLength)
                throw new ArgumentException($"Text exceeds maximum length of {MaxTextLength} characters.");

            var text = PrepareText(rawText);
            var key = ComputeHash("local_model" + "|" + text);
            var cacheKey = $"hf_local_{key}";
            if (_cache.TryGetValue(cacheKey, out AnalyzeAiResponseDTO cached))
                return cached;

            var overflowingChunks = EncodeWithOverflowing(text, MaxTokens, OverlapTokens);

            var chunkResults = new List<ChunkResultDTO>();
            double sumAiPercent = 0;
            int ok = 0;
            int chunkId = 1;
            int totalTokens = 0;

            foreach (var (inputIds, attentionMask, realCount) in overflowingChunks)
            {
                var prob = InferMachineProbabilityLocal(inputIds, attentionMask);
                if (prob.HasValue)
                {
                    sumAiPercent += prob.Value * 100.0;
                    ok++;
                    totalTokens += realCount;
                    chunkResults.Add(new ChunkResultDTO
                    {
                        ChunkId = chunkId++,
                        TokenCount = realCount,
                        ScoreMachine = Math.Round(prob.Value * 100.0, 2),
                        ScoreHuman = Math.Round(100 - prob.Value * 100.0, 2)
                    });
                }
            }

            var aiPercent = ok > 0 ? sumAiPercent / ok : 0.0;
            var aiTokenEquiv = totalTokens * (aiPercent / 100.0);

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
            if (string.IsNullOrWhiteSpace(chunk.Text))
                return null;
            if (chunk.Text.Length > MaxTextLength)
                throw new ArgumentException($"Chunk text exceeds maximum length of {MaxTextLength} characters.");

            var text = PrepareText(chunk.Text);

            var key = ComputeHash("local_chunk_model" + "|" + text + "|" + chunk.ChunkId);
            var cacheKey = $"hf_local_chunk_{key}";
            if (_cache.TryGetValue(cacheKey, out ChunkResultDTO cached))
                return cached;

            var overflowingSubChunks = EncodeWithOverflowing(text, MaxTokens, OverlapTokens);

            double sumProb = 0;
            int subOk = 0;
            int totalSubTokens = 0;

            foreach (var (inputIds, attentionMask, realCount) in overflowingSubChunks)
            {
                var prob = InferMachineProbabilityLocal(inputIds, attentionMask);
                if (prob.HasValue)
                {
                    sumProb += prob.Value;
                    subOk++;
                    totalSubTokens += realCount;
                }
            }

            var avgProb = subOk > 0 ? sumProb / subOk : 0.0;

            var result = new ChunkResultDTO
            {
                ChunkId = chunk.ChunkId,
                TokenCount = totalSubTokens,
                ScoreMachine = Math.Round(avgProb * 100.0, 2),
                ScoreHuman = Math.Round(100 - avgProb * 100.0, 2)
            };

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }

        /// <summary>
        /// Nhận uint[] từ tokenizer; chuyển sang long[] khi tạo tensor cho ONNX.
        /// </summary>
        private double? InferMachineProbabilityLocal(uint[] inputIds, int[] attentionMask)
        {
            // ONNX thường nhận int64 (long)
            var idsLong = inputIds.Select(x => (long)x).ToArray();
            var maskLong = attentionMask.Select(x => (long)x).ToArray();

            var inputTensor = new DenseTensor<long>(idsLong, new[] { 1, idsLong.Length });
            var maskTensor = new DenseTensor<long>(maskLong, new[] { 1, maskLong.Length });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>().ToArray();

            // Softmax
            // (dùng double để tránh tràn, nhưng input là float)
            double maxLogit = output.Max();
            var expScores = output.Select(x => Math.Exp(x - maxLogit)).ToArray(); // ổn định số học
            var sumExp = expScores.Sum();
            var probabilities = expScores.Select(x => x / sumExp).ToArray();

            // giả định index 1 là xác suất "AI-generated"
            if (probabilities.Length == 0) return null;
            return probabilities.Length > 1 ? probabilities[1] : probabilities[0];
        }
    }
}
