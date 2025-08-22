namespace ConferenceFWebAPI.DTOs.AICheckDTO
{
    public class AnalyzeAiRequestDTO
    {
        public int? ReviewId { get; set; } // ID của review, có thể null
        public string RawText { get; set; } = string.Empty; // Văn bản thô từ FE
    }

    public class ChunkPayloadDTO
    {
        public int ChunkId { get; set; } // ID của chunk
        public string Text { get; set; } = string.Empty; // Nội dung văn bản của chunk
        public int TokenCount { get; set; } // Số token trong chunk
        public string? Hash { get; set; } // Hash của chunk (có thể null)
    }
}