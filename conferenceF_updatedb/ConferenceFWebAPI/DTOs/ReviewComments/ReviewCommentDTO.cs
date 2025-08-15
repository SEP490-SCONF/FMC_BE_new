namespace ConferenceFWebAPI.DTOs.ReviewComments
{
    public class ReviewCommentDTO
    {
        public int CommentId { get; set; }

        public int? HighlightId { get; set; }

        public int ReviewId { get; set; }

        public int UserId { get; set; }

        public string? CommentText { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
