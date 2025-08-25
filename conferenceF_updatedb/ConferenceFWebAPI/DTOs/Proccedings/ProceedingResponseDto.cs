namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingResponseDto
    {
        public int ProceedingId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? CoverPageUrl { get; set; } 
        public string? Doi { get; set; }
        public string? Status { get; set; }
        public string? Version { get; set; }
        public DateTime? PublishedDate { get; set; }
        public UserInfoDto? PublishedBy { get; set; }
        public List<PaperInfoDto>? Papers { get; set; }
    }
    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
    }

    public class PaperInfoDto
    {
        public int PaperId { get; set; }
        public string? Title { get; set; }
    }
}
