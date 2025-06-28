using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Conferences
{
    public class ConferenceResponseDTO
    {
        [Required]
        public string ConferenceId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int CreatedBy { get; set; }
        public string? CallForPaper { get; set; }

        public string? BannerUrl { get; set; }
        public bool? Status { get; set; }

        public List<TopicDTO>? Topics { get; set; }

    }
}
