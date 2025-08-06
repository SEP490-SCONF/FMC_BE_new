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

            int totalTokens = 0;
            double weightedAiTokens = 0;
            var chunkResults = new List<ChunkResultDTO>();

            foreach (var chunk in request.Chunks)
            {
                totalTokens += chunk.TokenCount;

                var result = await _detector.AnalyzeChunkAsync(new ChunkPayloadDTO
                {
                    ChunkId = chunk.ChunkId,
                    Text = chunk.Text,
                    TokenCount = chunk.TokenCount,
                    Hash = chunk.Hash
                });

                if (result == null)
                {
                    // Nếu service trả null, bỏ qua hoặc giả định human
                    continue;
                }

                weightedAiTokens += result.TokenCount * result.ScoreMachine;
                chunkResults.Add(result);
            }

            double percentAi = totalTokens > 0
                ? (weightedAiTokens / totalTokens) * 100.0
                : 0.0;

            var response = new AnalyzeAiResponseDTO
            {
                PercentAi = Math.Round(percentAi, 2),
                TotalTokens = totalTokens,
                AiTokenEquiv = Math.Round(weightedAiTokens, 2),
                Chunks = chunkResults
            };

            return Ok(response);
        }
    }
}
