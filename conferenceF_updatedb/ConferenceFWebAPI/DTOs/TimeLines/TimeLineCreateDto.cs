using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.TimeLines
{
    public class TimeLineCreateDto
    {
        [Required]
        public int ConferenceId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }
}
