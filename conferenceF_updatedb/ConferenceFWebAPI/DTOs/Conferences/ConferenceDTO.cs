using DataAccess;
using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Conferences
{
    public class ConferenceDTO
    {
        public int ConferenceId { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int CreatedBy { get; set; }
        public string? CallForPaper { get; set; }
        public bool? Status { get; set; }
        public IFormFile? BannerImage { get; set; } 



    }
}
