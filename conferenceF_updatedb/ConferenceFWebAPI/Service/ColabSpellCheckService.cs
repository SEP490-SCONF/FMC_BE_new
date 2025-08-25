using System.Text.Json;

namespace ConferenceFWebAPI.Service
{
    public class ColabSpellCheckService : IAiSpellCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ColabSpellCheckService> _logger;

        public ColabSpellCheckService(HttpClient httpClient, IConfiguration config, ILogger<ColabSpellCheckService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<string> CheckSpellingAsync(string text)
        {
            var url = _config["Colab:SpellCheckUrl"];
            var apiKey = _config["Colab:ApiKey"];
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Colab:SpellCheckUrl is not configured.");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Colab:ApiKey is not configured.");

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("X-API-Key", apiKey);
            req.Content = JsonContent.Create(new { text = text });

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            _logger.LogInformation("Colab raw response: {raw}", raw);

            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"Colab API error {(int)resp.StatusCode}: {raw}");
            }

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("corrected", out var corrected))
                return corrected.GetString() ?? string.Empty;

            if (doc.RootElement.TryGetProperty("error", out var err))
                throw new Exception(err.GetString());

            return raw;
        }


        public async Task<List<string>> GetMisspelledWordsAsync(string text)
        {
            var url = _config["Colab:SpellCheckUrl"];
            var apiKey = _config["Colab:ApiKey"];
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Colab:SpellCheckUrl is not configured.");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Colab:ApiKey is not configured.");

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("X-API-Key", apiKey);
            req.Content = JsonContent.Create(new { text = text });

            var resp = await _httpClient.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            _logger.LogInformation("Colab raw response: {raw}", raw);

            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"Colab API error {(int)resp.StatusCode}: {raw}");
            }

            using var doc = JsonDocument.Parse(raw);

            // Nếu API có trả về danh sách misspelledWords
            if (doc.RootElement.TryGetProperty("misspelledWords", out var misspelledWordsElement))
            {
                var misspelledWords = new List<string>();
                foreach (var wordElem in misspelledWordsElement.EnumerateArray())
                {
                    if (wordElem.ValueKind == JsonValueKind.String)
                        misspelledWords.Add(wordElem.GetString() ?? "");
                }
                return misspelledWords;
            }

            // Nếu không có trường misspelledWords, trả về rỗng
            return new List<string>();
        }

    }
}
