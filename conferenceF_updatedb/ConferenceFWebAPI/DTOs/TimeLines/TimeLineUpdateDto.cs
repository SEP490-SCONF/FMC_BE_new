using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.TimeLines
{
    public class TimeLineUpdateDto
    {
        [Required]
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }
}
