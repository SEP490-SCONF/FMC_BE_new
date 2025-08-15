using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.CallForPapers
{
    public class CallForPaperUpdateDto
    {


        [Required(ErrorMessage = "ConferenceId là bắt buộc.")]
        public int ConferenceId { get; set; }

        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }
        public bool Status { get; set; }


        public IFormFile? TemplateFile { get; set; } // Optional: Nếu có file mới, sẽ ghi đè


    }
}
