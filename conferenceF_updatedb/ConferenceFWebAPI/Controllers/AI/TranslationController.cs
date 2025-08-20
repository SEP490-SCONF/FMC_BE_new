using ConferenceFWebAPI.DTOs.AICheckDTO;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly DeepLTranslationService _translationService;

        public TranslationController(DeepLTranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate-highlight")]
        public async Task<IActionResult> TranslateHighlightedText([FromForm] TranslationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text to translate cannot be empty.");

            try
            {
                string text = request.Text.Replace("\r\n", "\n"); // normalize newline
                var lines = text.Split('\n', StringSplitOptions.None);
                var translatedLines = new List<string>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        translatedLines.Add(""); // giữ dòng trống
                        continue;
                    }

                    // Chia line dài thành chunk nhỏ (ví dụ 200 ký tự) để dịch
                    var chunks = SplitIntoChunks(line, 200);
                    var translatedChunks = new List<string>();
                    foreach (var chunk in chunks)
                    {
                        var translated = await _translationService.TranslateAsync(chunk, request.TargetLanguage ?? "en-US");
                        translatedChunks.Add(translated.Trim());
                    }

                    translatedLines.Add(string.Join(" ", translatedChunks));
                }

                var translatedText = string.Join("\n", translatedLines);

                return Ok(new { TranslatedText = translatedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // Hàm chia chunk
        private List<string> SplitIntoChunks(string text, int maxLength)
        {
            var result = new List<string>();
            for (int i = 0; i < text.Length; i += maxLength)
            {
                result.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
            }
            return result;
        }



    }
}
