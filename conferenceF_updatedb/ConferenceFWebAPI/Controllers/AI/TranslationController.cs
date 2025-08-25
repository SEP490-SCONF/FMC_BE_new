using Azure;
using ConferenceFWebAPI.DTOs.AICheckDTO;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly AzureTranslationService _translationService;

        public TranslationController(AzureTranslationService translationService)
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
                // Tách text thành từng dòng
                var lines = request.Text.Replace("\r\n", "\n").Split('\n');

                var translatedLines = new List<string>();
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var translated = await _translationService.TranslateAsync(
                            line.Trim(),
                            request.TargetLanguage ?? "en"
                        );
                        translatedLines.Add(translated.Trim());
                    }
                }

                // Ghép các dòng đã dịch bằng '\n'
                string finalText = string.Join("\n", translatedLines);

                return new JsonResult(new { translatedText = finalText });
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

    }
}
