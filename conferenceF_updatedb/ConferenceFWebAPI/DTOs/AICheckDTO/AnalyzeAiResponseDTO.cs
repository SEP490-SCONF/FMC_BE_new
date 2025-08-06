namespace ConferenceFWebAPI.DTOs.AICheckDTO
{
    public class AnalyzeAiResponseDTO
    {
        public double PercentAi { get; set; }
        public int TotalTokens { get; set; }
        public double AiTokenEquiv { get; set; }
        public List<ChunkResultDTO> Chunks { get; set; } = new();
    }

    public class ChunkResultDTO
    {
        public int ChunkId { get; set; }
        public int TokenCount { get; set; }
        public double ScoreMachine { get; set; }
        public double ScoreHuman { get; set; }
    }

}
