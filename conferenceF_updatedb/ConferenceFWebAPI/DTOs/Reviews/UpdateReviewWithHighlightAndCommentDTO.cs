namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class UpdateReviewWithHighlightAndCommentDTO
    {
        public int? Score { get; set; }
        public string? Comments { get; set; }

        public int RevisionId { get; set; } // để truy lại PaperId

        // Highlight
        public int HighlightId { get; set; }  
        public int PageIndex { get; set; }   
        public double Left { get; set; }     
        public double Top { get; set; }     
        public double Width { get; set; }     
        public double Height { get; set; }    
        public string? TextHighlighted { get; set; }  

        // Comment
        public int CommentId { get; set; } // cần biết để update
        public string? CommentText { get; set; }
        public string? Status { get; set; }
    }
}
