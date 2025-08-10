using DeepL;

namespace ConferenceFWebAPI.Service
{
    public class DeepLTranslationService
    {
        private readonly Translator _translator;

        public DeepLTranslationService(IConfiguration configuration)
        {
            var authKey = configuration["DeepL:AuthKey"];
            _translator = new Translator(authKey);
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage = "en")
        {
            var result = await _translator.TranslateTextAsync(text, sourceLanguage, targetLanguage);
            return result.Text;
        }
    }
}
