using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.CallForPapers
{
    public class CallForPaperUpdateDto
    {
        [Required(ErrorMessage = "Cfpid là bắt buộc để cập nhật.")]
        public int Cfpid { get; set; } 

        [Required(ErrorMessage = "ConferenceId là bắt buộc.")]
        public int ConferenceId { get; set; }

        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }

        public IFormFile? TemplateFile { get; set; } // Optional: Nếu có file mới, sẽ ghi đè


    }
}
