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
        public async Task<IActionResult> TranslateHighlightedText([FromBody] TranslationRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest("Text to translate cannot be empty.");
            }

            try
            {
                var translatedText = await _translationService.TranslateAsync(
                    request.Text,
                    request.TargetLanguage ?? "en" // Ngôn ngữ đích mặc định
                );

                return Ok(new { TranslatedText = translatedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
