using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs
{
    public class PaperCreateDto
    {
        [Required]
        public int ConferenceId { get; set; }  // ✅ Thêm ConferenceId
        [Required]
        public string Title { get; set; }

        public string? Abstract { get; set; }

        public string? Keywords { get; set; }

        public int? TopicId { get; set; }

        [Required]
        public int AuthorId { get; set; }  // 👈 Chỉ 1 author

        [Required]
        public IFormFile File { get; set; }
    }
}
