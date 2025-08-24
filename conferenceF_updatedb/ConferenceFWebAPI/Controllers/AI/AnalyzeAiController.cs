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
            if (request == null || (string.IsNullOrWhiteSpace(request.RawText) && (request.Chunks == null || !request.Chunks.Any())))
            {
                return BadRequest(new { error = "No raw text or chunks provided." });
            }

            try
            {
                if (request.Chunks != null && request.Chunks.Any())
                {
                    // Process chunks
                    var chunkResults = new List<ChunkResultDTO>();
                    double totalWeightedAiScore = 0;
                    int totalTokens = 0;

                    foreach (var chunk in request.Chunks)
                    {
                        var chunkResult = await _detector.AnalyzeChunkAsync(chunk);
                        if (chunkResult != null)
                        {
                            chunkResults.Add(chunkResult);
                            totalWeightedAiScore += chunkResult.ScoreMachine * chunkResult.TokenCount;
                            totalTokens += chunkResult.TokenCount;
                        }
                    }

                    double percentAi = totalTokens > 0 ? totalWeightedAiScore / totalTokens : 0.0;
                    double aiTokenEquiv = totalTokens * (percentAi / 100.0);

                    var analysisResult = new AnalyzeAiResponseDTO
                    {
                        PercentAi = Math.Round(percentAi, 2),
                        TotalTokens = totalTokens,
                        AiTokenEquiv = Math.Round(aiTokenEquiv, 2),
                        Chunks = chunkResults
                    };

                    return Ok(analysisResult);
                }
                else
                {
                    // Fallback to raw text if no chunks
                    var analysisResult = await _detector.AnalyzeTextAsync(request.RawText);
                    return Ok(analysisResult);
                }
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