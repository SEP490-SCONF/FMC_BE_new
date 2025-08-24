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

namespace ConferenceFWebAPI.Service
{
    public class HuggingFaceDetectorService : IAiDetectorService
    {
        private readonly IMemoryCache _cache;
        private readonly InferenceSession _session;
        private readonly Tokenizer _tokenizer;
        private const int MaxTokens = 512; // Giới hạn tối đa 512 token
        private const int OverlapTokens = 128; // Overlap 128 token
        private const int MaxTextLength = 50000; // Giới hạn tối đa 50,000 ký tự (~20 trang)

        public HuggingFaceDetectorService(IMemoryCache cache)
        {
            _cache = cache;

            // Khởi tạo session ONNX
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "model.onnx");
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Model file not found at " + modelPath);
            _session = new InferenceSession(modelPath);

            // Khởi tạo tokenizer với vocab và tokenizer files
            var vocabPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "vocab.json");
            var tokenizerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "tokenizer.json");
            if (!File.Exists(vocabPath) || !File.Exists(tokenizerPath))
                throw new FileNotFoundException("Tokenizer files not found.");
            _tokenizer = new Tokenizer(vocabPath, tokenizerPath);
        }

        public class Tokenizer
        {
            private readonly Dictionary<string, int> _vocab;
            private readonly List<(string, string)> _merges;
            private readonly Dictionary<(string, string), int> _mergeRanks;

            public Tokenizer(string vocabPath, string tokenizerPath)
            {
                // vocab
                var vocabJson = File.ReadAllText(vocabPath);
                _vocab = JsonSerializer.Deserialize<Dictionary<string, int>>(vocabJson) ?? new();

                // merges
                var tokenizerJson = File.ReadAllText(tokenizerPath);
                var tokenizerData = JsonDocument.Parse(tokenizerJson).RootElement;
                _merges = new();
                _mergeRanks = new();
                if (tokenizerData.TryGetProperty("merges", out var mergesArray))
                {
                    int rank = 0;
                    foreach (var merge in mergesArray.EnumerateArray())
                    {
                        var pair = merge.GetString()?.Split(' ');
                        if (pair?.Length == 2)
                        {
                            _merges.Add((pair[0], pair[1]));
                            _mergeRanks[(pair[0], pair[1])] = rank++;
                        }
                    }
                }
            }

            public List<(int[] inputIds, int[] attentionMask, int realTokenCount)>
                EncodeWithOverflowing(string text, int maxLength = 512, int stride = 128)
            {
                // bỏ hex hóa, dùng từng ký tự unicode
                var tokens = text.Select(c => c.ToString()).ToList();

                // BPE chuẩn
                tokens = ApplyBpe(tokens);

                var chunks = new List<(int[] inputIds, int[] attentionMask, int realTokenCount)>();
                for (int start = 0; start < tokens.Count; start += (maxLength - stride))
                {
                    var end = Math.Min(start + maxLength, tokens.Count);
                    var chunkTokens = tokens.Skip(start).Take(end - start).ToArray();

                    var inputIds = chunkTokens
                        .Select(t => _vocab.GetValueOrDefault(t, _vocab.GetValueOrDefault("<unk>", 0)))
                        .ToList();

                    int realCount = inputIds.Count;

                    // pad
                    if (realCount < maxLength)
                        inputIds.AddRange(Enumerable.Repeat(0, maxLength - realCount));

                    var attentionMask = inputIds.Select((id, idx) => idx < realCount ? 1 : 0).ToArray();

                    chunks.Add((inputIds.ToArray(), attentionMask, realCount));

                    if (end >= tokens.Count) break;
                }

                Console.WriteLine($"Total tokens before chunking: {tokens.Count}, Number of chunks: {chunks.Count}");
                return chunks;
            }

            private List<string> ApplyBpe(List<string> tokens)
            {
                var word = new List<string>(tokens);
                while (true)
                {
                    (string, string)? best = null;
                    int bestRank = int.MaxValue;

                    // tìm cặp merge có rank thấp nhất (ưu tiên cao nhất)
                    for (int i = 0; i < word.Count - 1; i++)
                    {
                        var pair = (word[i], word[i + 1]);
                        if (_mergeRanks.TryGetValue(pair, out var rank) && rank < bestRank)
                        {
                            best = pair;
                            bestRank = rank;
                        }
                    }

                    if (best == null) break;

                    // merge cặp tốt nhất
                    var newWord = new List<string>();
                    int idx = 0;
                    while (idx < word.Count)
                    {
                        int j = idx + 1;
                        if (j < word.Count && (word[idx], word[j]) == best)
                        {
                            newWord.Add(word[idx] + word[j]);
                            idx += 2;
                        }
                        else
                        {
                            newWord.Add(word[idx]);
                            idx++;
                        }
                    }
                    word = newWord;
                }
                return word;
            }
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
            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\u22A4", "transpose")
                .Replace("[^\\w\\s.,!?()'-=₀-₉]", "");
        }

        public async Task<AnalyzeAiResponseDTO> AnalyzeTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                throw new ArgumentException("Raw text is required.");

            if (rawText.Length > MaxTextLength)
                throw new ArgumentException($"Text exceeds maximum length of {MaxTextLength} characters.");

            var text = PrepareText(rawText);
            Console.WriteLine($"Raw text length: {rawText.Length}, Prepared text length: {text.Length}, Content: {text.Substring(0, Math.Min(100, text.Length))}...");

            var key = ComputeHash("local_model" + "|" + text);
            var cacheKey = $"hf_local_{key}";
            if (_cache.TryGetValue(cacheKey, out AnalyzeAiResponseDTO cached))
                return cached;

            // Sử dụng EncodeWithOverflowing để tạo chunks với overlap
            var overflowingChunks = _tokenizer.EncodeWithOverflowing(text, MaxTokens, OverlapTokens);
            var firstChunk = overflowingChunks.FirstOrDefault();
            int firstChunkTokenCount = firstChunk.inputIds != null ? firstChunk.inputIds.Length : 0;
            Console.WriteLine($"Number of chunks created: {overflowingChunks.Count}, First chunk token count: {firstChunkTokenCount}");

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
                    totalTokens += realCount; // chỉ cộng số token thật, không cộng padding
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

        public Task<ChunkResultDTO?> AnalyzeChunkAsync(ChunkPayloadDTO chunk)
        {
            return Task.FromResult<ChunkResultDTO?>(null);
        }

        private double? InferMachineProbabilityLocal(int[] inputIds, int[] attentionMask)
        {
            var inputTensor = new DenseTensor<long>(new[] { 1, inputIds.Length });
            inputIds.Select(x => (long)x).ToArray().CopyTo(inputTensor.Buffer.Span);

            var maskTensor = new DenseTensor<long>(new[] { 1, attentionMask.Length });
            attentionMask.Select(x => (long)x).ToArray().CopyTo(maskTensor.Buffer.Span);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>().ToArray();

            // Áp dụng softmax
            var expScores = output.Select(x => Math.Exp(x)).ToArray();
            var sumExp = expScores.Sum();
            var probabilities = expScores.Select(x => x / sumExp).ToArray();
            Console.WriteLine($"Output length: {output.Length}, Probabilities: {string.Join(", ", probabilities)}");

            // Lấy xác suất AI-generated (index 1 theo thông tin mới)
            return probabilities.Length > 1 ? probabilities[1] : probabilities[0];
        }
    }
}