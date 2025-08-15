namespace ConferenceFWebAPI.DTOs.ReviewHightlights
{
    public class AddReviewHighlightWithCommentDTO
    {
        public int ReviewId { get; set; }
        public int? PageNumber { get; set; }
        public int? OffsetStart { get; set; }
        public int? OffsetEnd { get; set; }
        public string? TextHighlighted { get; set; }

        // Thông tin Comment
        public int UserId { get; set; }
        public string? CommentText { get; set; }
        public string? Status { get; set; }
    }
}
