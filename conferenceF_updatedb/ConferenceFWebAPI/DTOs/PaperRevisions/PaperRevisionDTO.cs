namespace ConferenceFWebAPI.DTOs.PaperRevisions
{
    public class PaperRevisionDTO
    {
        public int RevisionId { get; set; }

        public int PaperId { get; set; }

        public string? FilePath { get; set; }

        public string? Status { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string? Comments { get; set; }
    }
}
