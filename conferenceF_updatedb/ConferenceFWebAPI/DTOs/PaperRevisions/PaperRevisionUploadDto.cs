using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.PaperRevisions
{
    public class PaperRevisionUploadDto
    {
        [Required]
        public int PaperId { get; set; } // Liên kết bản sửa đổi với một Paper cụ thể

        [Required]
        public IFormFile PdfFile { get; set; } = null!;

        public string? Comments { get; set; } // Bình luận cho bản sửa đổi này
    }
}
