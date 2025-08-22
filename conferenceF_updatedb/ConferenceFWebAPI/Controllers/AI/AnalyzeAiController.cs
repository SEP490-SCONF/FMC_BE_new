using ConferenceFWebAPI.DTOs.AICheckDTO;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzeAiController : ControllerBase
    {
        private readonly IAiDetectorService _detector;

        public AnalyzeAiController(IAiDetectorService detector)
        {
            _detector = detector;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AnalyzeAiRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RawText))
            {
                return BadRequest(new { error = "No raw text provided." });
            }

            try
            {
                var analysisResult = await _detector.AnalyzeTextAsync(request.RawText);
                return Ok(analysisResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}