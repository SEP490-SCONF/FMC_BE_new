using ConferenceFWebAPI.DTOs.AICheckDTO;

namespace ConferenceFWebAPI.Service
{
    public interface IAiDetectorService
    {
        Task<AnalyzeAiResponseDTO> AnalyzeTextAsync(string rawText);
        Task<ChunkResultDTO?> AnalyzeChunkAsync(ChunkPayloadDTO chunk);
    }
}
