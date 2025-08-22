namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingCreateFromFormDto
    {
        public int ConferenceId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? Doi { get; set; }
        public int PublishedBy { get; set; }
        public string? PaperIds { get; set; }
    }
}
