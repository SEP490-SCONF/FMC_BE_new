namespace ConferenceFWebAPI.DTOs
{
    public class PaperDto
    {
        public int PaperId { get; set; }
        public string Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? TopicId { get; set; }
        public string? FilePath{ get; set; }  // ✅ ánh xạ từ FilePath
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }
        public bool? IsPublished { get; set; }
        public decimal? PublicationFee { get; set; }

        public string? TopicName { get; set; }           // từ Topic.Name
        public List<string>? AuthorNames { get; set; }   // từ Authors.FullName
    }
}
