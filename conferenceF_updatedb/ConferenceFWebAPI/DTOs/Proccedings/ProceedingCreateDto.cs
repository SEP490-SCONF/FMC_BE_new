namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingCreateDto
    {
        public int ConferenceId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? Doi { get; set; }
        public int PublishedBy { get; set; }
        public List<int>? PaperIds { get; set; } // Danh sách các PaperId để liên kết
    }

}
