namespace ConferenceFWebAPI.DTOs
{
    public class ConferenceDTO
    {
       
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string? BannerUrl { get; set; }
        public bool? CallForPaper { get; set; }
    }
}
