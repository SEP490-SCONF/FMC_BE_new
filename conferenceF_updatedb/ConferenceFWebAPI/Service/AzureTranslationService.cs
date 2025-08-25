using Azure.AI.Translation.Text;
using Azure;

namespace ConferenceFWebAPI.Service
{
    public class AzureTranslationService
    {
        private readonly TextTranslationClient _client;

        public AzureTranslationService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureTranslator:Endpoint"];
            var key = configuration["AzureTranslator:Key"];

            // Tạo một đối tượng AzureKeyCredential từ khóa dịch vụ.
            var credential = new AzureKeyCredential(key);

            // Khởi tạo client dịch thuật với endpoint và credential.
            _client = new TextTranslationClient(credential, new System.Uri(endpoint));
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage = "en")
        {
            // Wrap the single text string into a list or array.
            var textsToTranslate = new List<string> { text };

            // Pass the arguments by position, not by a named parameter that doesn't exist.
            var response = await _client.TranslateAsync(
                targetLanguages: new string[] { targetLanguage },
                textsToTranslate, // <-- This is the corrected line
                sourceLanguage: sourceLanguage
            );

            var translatedText = response.Value[0].Translations[0].Text;
            return translatedText;
        }
    }
}
