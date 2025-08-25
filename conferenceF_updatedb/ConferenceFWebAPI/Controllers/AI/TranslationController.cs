using Azure;
using ConferenceFWebAPI.DTOs.AICheckDTO;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text to translate cannot be empty.");

            try
            {
                // Chuẩn hóa newline
                string text = request.Text.Replace("\r\n", "\n");

                // Dịch nguyên cả block text
                string translated = await _translationService.TranslateAsync(
                    text,
                    request.TargetLanguage ?? "en"
                );

                // Loại bỏ thẻ HTML nếu có
                translated = Regex.Replace(translated, @"<br\s*/?>", " ").Trim();

                // Thêm logic xuống dòng sau mỗi 10 từ
                string[] words = translated.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var formattedText = new StringBuilder();

                for (int i = 0; i < words.Length; i++)
                {
                    formattedText.Append(words[i]);

                    // Nếu không phải từ cuối cùng và đủ 10 từ, thêm ký tự xuống dòng
                    if ((i + 1) % 10 == 0 && i < words.Length - 1)
                    {
                        formattedText.Append("\n");
                    }
                    // Thêm khoảng trắng sau mỗi từ (trừ từ cuối cùng)
                    else if (i < words.Length - 1)
                    {
                        formattedText.Append(" ");
                    }
                }

                return new JsonResult(new { translatedText = formattedText.ToString() });
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