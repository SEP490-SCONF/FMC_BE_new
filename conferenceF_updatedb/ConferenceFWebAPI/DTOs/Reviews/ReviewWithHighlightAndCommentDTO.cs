using ConferenceFWebAPI.DTOs.Paper;
using ConferenceFWebAPI.DTOs.ReviewComments;
using ConferenceFWebAPI.DTOs.ReviewHightlights;

namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class ReviewWithHighlightAndCommentDTO
    {
        // Review
        public int ReviewId { get; set; }
        public int? PaperId { get; set; }
        public int ReviewerId { get; set; }
        public int RevisionId { get; set; }
        public int? Score { get; set; }
        public string? Comments { get; set; }
        public string? Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RevisionStatus { get; set; }
        public string? FilePath { get; set; }


        public List<ReviewHightlightDTO> Highlights { get; set; } = new();

        public List<ReviewCommentDTO> CommentsList { get; set; } = new();
        public PaperResponseDto? Paper { get; set; }

    }
}

