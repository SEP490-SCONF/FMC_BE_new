namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingCreateDto
    {
        public int ConferenceId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile File { get; set; } = null!;
    }

}
