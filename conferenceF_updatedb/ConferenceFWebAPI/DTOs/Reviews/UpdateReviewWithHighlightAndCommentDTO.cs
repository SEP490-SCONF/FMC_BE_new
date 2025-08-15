namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class UpdateReviewWithHighlightAndCommentDTO
    {
        public int? Score { get; set; }
        public string? Comments { get; set; }
        public int RevisionId { get; set; } // để truy lại PaperId

        // Highlight
        public int HighlightId { get; set; }
        public string? TextHighlighted { get; set; }

        // Danh sách các vùng highlight (nhiều vùng)
        public List<HighlightAreaDTO> HighlightAreas { get; set; } = new();

        // Comment
        public int CommentId { get; set; } // cần biết để update
        public string? CommentText { get; set; }
        public string? Status { get; set; }
    }
}
