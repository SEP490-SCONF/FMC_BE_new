namespace ConferenceFWebAPI.DTOs
{
    public class ConferenceDTO
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Location { get; set; }

        public int CreatedBy { get; set; }

        public string? BannerUrl { get; set; }

        public string? CallForPaper { get; set; }
    }
}
