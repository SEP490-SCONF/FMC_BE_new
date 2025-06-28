namespace ConferenceFWebAPI.DTOs.Papers
{
    public class PaperResponseWT
    {
        public int PaperId { get; set; } // Giữ lại ID để xác định bài báo

        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        
        public string TopicName { get; set; }
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }
    }
}
