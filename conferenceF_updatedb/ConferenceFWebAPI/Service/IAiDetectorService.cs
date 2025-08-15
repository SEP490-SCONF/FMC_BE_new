using ConferenceFWebAPI.DTOs.AICheckDTO;

namespace ConferenceFWebAPI.Service
{
    public interface IAiDetectorService
    {
        Task<ChunkResultDTO?> AnalyzeChunkAsync(ChunkPayloadDTO chunk);
    }
}
