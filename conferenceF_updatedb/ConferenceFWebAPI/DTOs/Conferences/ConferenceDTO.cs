using DataAccess;
using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Conferences
{
    public class ConferenceDTO
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int CreatedBy { get; set; }
        public string? CallForPaper { get; set; }

        public IFormFile? BannerImage { get; set; }


    }
}
