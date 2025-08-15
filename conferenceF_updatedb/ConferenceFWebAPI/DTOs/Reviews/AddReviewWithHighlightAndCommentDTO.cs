namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class AddReviewWithHighlightAndCommentDTO
    {
        public int ReviewId { get; set; }
        public int ReviewerId { get; set; }
        public int RevisionId { get; set; }
        public int? Score { get; set; }
        public string? Comments { get; set; }
        public string? Quote { get; set; } // Thêm quote nếu cần

        // Highlight areas (nhiều vùng)
        public List<HighlightAreaDTO> HighlightAreas { get; set; }

        // Comment
        public int UserId { get; set; }
        public string? CommentText { get; set; }
        public string? Status { get; set; }
    }
}
