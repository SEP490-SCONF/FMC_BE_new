using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Papers
{
    public class PaperUploadDto
    {
        // Các trường dữ liệu của Paper mà bạn muốn nhận từ người dùng
        [Required(ErrorMessage = "Conference ID is required.")]
        public int ConferenceId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Abstract cannot exceed 2000 characters.")]
        public string? Abstract { get; set; }

        [StringLength(500, ErrorMessage = "Keywords cannot exceed 500 characters.")]
        public string? Keywords { get; set; }

        public int? TopicId { get; set; }

        [Required] // Đảm bảo bạn nhận được ít nhất một tác giả
        public List<int> AuthorIds { get; set; } = new List<int>();

        // Trường để nhận file PDF từ client
        [Required(ErrorMessage = "Please select a file.")]
        [DataType(DataType.Upload)]
        [MaxFileSize(30 * 1024 * 1024)] // Ví dụ: 5MB giới hạn kích thước file
        [AllowedExtensions(new string[] { ".pdf" })] // Chỉ cho phép file PDF
        public IFormFile PdfFile { get; set; }

        public class MaxFileSizeAttribute : ValidationAttribute
        {
            private readonly int _maxFileSize;
            public MaxFileSizeAttribute(int maxFileSize)
            {
                _maxFileSize = maxFileSize;
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var file = value as IFormFile;
                if (file != null)
                {
                    if (file.Length > _maxFileSize)
                    {
                        return new ValidationResult(GetErrorMessage());
                    }
                }
                return ValidationResult.Success;
            }

            public string GetErrorMessage()
            {
                return $"Maximum allowed file size is {_maxFileSize / 1024 / 1024} MB.";
            }
        }

        public class AllowedExtensionsAttribute : ValidationAttribute
        {
            private readonly string[] _extensions;
            public AllowedExtensionsAttribute(string[] extensions)
            {
                _extensions = extensions.Select(e => e.ToLower()).ToArray(); // Chuyển đổi sang chữ thường
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var file = value as IFormFile;
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName)?.ToLower(); // Lấy phần mở rộng và chuyển sang chữ thường
                    if (extension == null || !_extensions.Contains(extension))
                    {
                        return new ValidationResult($"This file extension is not allowed! Allowed extensions are {string.Join(", ", _extensions)}");
                    }
                }
                return ValidationResult.Success;
            }
        }
    }
}
