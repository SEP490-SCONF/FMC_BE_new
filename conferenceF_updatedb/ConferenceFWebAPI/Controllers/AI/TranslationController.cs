using Azure;
using ConferenceFWebAPI.DTOs.AICheckDTO;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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
            // Logic của bạn vẫn giữ nguyên ở đây
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text to translate cannot be empty.");

            try
            {
                string text = request.Text.Replace("\r\n", "\n");

                string translated = await _translationService.TranslateAsync(
                    text,
                    request.TargetLanguage ?? "en"
                );

                // ... (phần còn lại của code)
                translated = Regex.Replace(translated, @"<br\s*/?>", " ").Trim();

                return new JsonResult(new { translatedText = translated });
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
