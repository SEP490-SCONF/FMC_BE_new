namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class AddReviewWithHighlightAndCommentDTO
    {
        public int ReviewId { get; set; }
        public int ReviewerId { get; set; }
        public int RevisionId { get; set; }
        public int? Score { get; set; }
        public string? Comments { get; set; }

        public int PageIndex { get; set; }  
        public double Left { get; set; }    
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? TextHighlighted { get; set; }  


        // Comment
        public int UserId { get; set; }
        public string? CommentText { get; set; }
        public string? Status { get; set; }
    }
}
