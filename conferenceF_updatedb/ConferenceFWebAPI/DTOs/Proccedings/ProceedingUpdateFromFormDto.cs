using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Proccedings
{
    public class ProceedingUpdateFromFormDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? PaperIds { get; set; } // Chuỗi PaperIds được phân tách bằng dấu phẩy

        public IFormFile? CoverImageFile { get; set; } // File ảnh bìa

        public int? PublishedBy { get; set; } // ID của người xuất bản

        public string? Doi { get; set; }
    }
}
