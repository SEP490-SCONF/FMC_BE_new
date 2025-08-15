namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingCreateFromPaperDto
    {
        public int ConferenceId { get; set; }
        public int PaperId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
