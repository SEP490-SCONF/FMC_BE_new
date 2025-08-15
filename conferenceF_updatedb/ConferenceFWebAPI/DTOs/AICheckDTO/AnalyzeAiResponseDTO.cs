namespace ConferenceFWebAPI.DTOs.AICheckDTO
{
    public class AnalyzeAiResponseDTO
    {
        public double PercentAi { get; set; } // Phần trăm nội dung AI (đã làm tròn 2 chữ số)
        public int TotalTokens { get; set; } // Tổng số token của tất cả chunk
        public double AiTokenEquiv { get; set; } // Giá trị token tương đương của AI
        public List<ChunkResultDTO> Chunks { get; set; } = new(); // Kết quả phân tích từng chunk
    }

    public class ChunkResultDTO
    {
        public int ChunkId { get; set; } // ID của chunk
        public int TokenCount { get; set; } // Số token trong chunk
        public double ScoreMachine { get; set; } // Điểm số dự đoán là máy
        public double ScoreHuman { get; set; } // Điểm số dự đoán là con người
    }
}