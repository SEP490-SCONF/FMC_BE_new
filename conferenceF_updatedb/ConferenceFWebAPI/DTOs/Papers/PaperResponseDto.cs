namespace ConferenceFWebAPI.DTOs.Paper
{
    public class PaperResponseDto
    {
        public int PaperId { get; set; } // Giữ lại ID để xác định bài báo

        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? TopicId { get; set; }
        public string? TopicName { get; set; } 

        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }

        public List<AuthorDto>? Authors { get; set; }
    }

    public class AuthorDto
    {
        public int AuthorId { get; set; } 
        public string? FullName { get; set; }     
        public int AuthorOrder { get; set; }
    }

}
