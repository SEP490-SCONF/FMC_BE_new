namespace ConferenceFWebAPI.DTOs.Conferences
{
    public class OrganizerConferenceDTO
    {
        public int ConferenceId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public bool? Status { get; set; }

        // Banner image URL hoặc path
        public string? BannerImage { get; set; }
    }

}
