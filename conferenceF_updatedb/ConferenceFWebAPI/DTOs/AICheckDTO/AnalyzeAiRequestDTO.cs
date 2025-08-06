namespace ConferenceFWebAPI.DTOs.AICheckDTO
{
    public class AnalyzeAiRequestDTO
    {
        public string? ReviewId { get; set; } 
        public List<ChunkPayloadDTO> Chunks { get; set; } = new();
    }

    public class ChunkPayloadDTO
    {
        public int ChunkId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int TokenCount { get; set; }
        public string? Hash { get; set; } 
    }

}
