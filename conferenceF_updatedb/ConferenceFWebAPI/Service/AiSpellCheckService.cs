using System.Net.Http.Headers;
using System.Text.Json;

namespace ConferenceFWebAPI.Service
{
    public class AiSpellCheckService : IAiSpellCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AiSpellCheckService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> CheckSpellingAsync(string text)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Không tìm thấy OpenAI:ApiKey trong cấu hình.");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "system", content = "You are a grammar and spelling correction assistant." },
            new { role = "user", content = $"Check grammar and spelling for the following text:\n\n{text}" }
        }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("OpenAI raw response: " + responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API Error ({(int)response.StatusCode}): {responseContent}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

    }
}
