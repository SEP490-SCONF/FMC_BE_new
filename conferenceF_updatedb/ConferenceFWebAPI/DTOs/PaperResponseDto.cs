namespace ConferenceFWebAPI.DTOs
{
    public class PaperResponseDto
    {
        public int PaperId { get; set; } // Giữ lại ID để xác định bài báo

        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? TopicId { get; set; }
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }
    }
}
