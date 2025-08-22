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
        private readonly DeepLTranslationService _translationService;

        public TranslationController(DeepLTranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate-highlight")]
        public async Task<IActionResult> TranslateHighlightedText([FromBody] TranslationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text to translate cannot be empty.");

            try
            {
                // Chuẩn hóa newline
                string text = request.Text.Replace("\r\n", "\n");

                // Dịch nguyên cả block text, không tách câu
                string translated = await _translationService.TranslateAsync(
                    text,
                    request.TargetLanguage ?? "en-US"
                );

                // Loại bỏ thẻ HTML nếu có
                translated = Regex.Replace(translated, @"<br\s*/?>", " ").Trim();

                // Giữ nguyên newline gốc
                // Trả về JSON, FE sẽ nhận dạng \n đúng
                return new JsonResult(new { translatedText = translated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
