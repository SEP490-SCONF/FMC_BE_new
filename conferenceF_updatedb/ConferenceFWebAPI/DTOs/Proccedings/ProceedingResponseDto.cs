namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingResponseDto
    {
        public int ProceedingId { get; set; }
        public string ConferenceTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? PublishedBy { get; set; }
        public string PublishedByName { get; set; }
    }


}
