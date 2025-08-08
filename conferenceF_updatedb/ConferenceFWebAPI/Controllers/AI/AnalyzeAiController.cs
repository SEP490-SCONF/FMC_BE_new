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
            if (request == null || request.Chunks == null || !request.Chunks.Any())
            {
                return BadRequest(new { error = "No chunks provided." });
            }

            var chunkResults = await ProcessChunksAsync(request.Chunks);
            var analysisResult = CalculateAnalysisResult(chunkResults);

            return Ok(analysisResult);
        }

        private async Task<List<ChunkResultDTO>> ProcessChunksAsync(IEnumerable<ChunkPayloadDTO> chunks)
        {
            var chunkResults = new List<ChunkResultDTO>();

            foreach (var chunk in chunks)
            {
                var result = await _detector.AnalyzeChunkAsync(chunk);

                if (result != null)
                {
                    chunkResults.Add(result);
                }
            }

            return chunkResults;
        }

        private AnalyzeAiResponseDTO CalculateAnalysisResult(List<ChunkResultDTO> chunkResults)
        {
            int totalTokens = chunkResults.Sum(r => r.TokenCount);
            double weightedAiTokens = chunkResults.Sum(r => r.TokenCount * r.ScoreMachine);
            double percentAi = totalTokens > 0
                ? (weightedAiTokens / totalTokens) * 100.0
                : 0.0;

            return new AnalyzeAiResponseDTO
            {
                PercentAi = Math.Round(percentAi, 2),
                TotalTokens = totalTokens,
                AiTokenEquiv = Math.Round(weightedAiTokens, 2),
                Chunks = chunkResults
            };
        }
    }
}