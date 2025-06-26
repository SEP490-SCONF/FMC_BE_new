using ConferenceFWebAPI.DTOs.ReviewComments;

namespace ConferenceFWebAPI.DTOs.ReviewHightlights
{
    public class ReviewHightlightDTO
    {
        public int HighlightId { get; set; }

        public int ReviewId { get; set; }

        public int? PageNumber { get; set; }

        public int? OffsetStart { get; set; }

        public int? OffsetEnd { get; set; }

        public string? TextHighlighted { get; set; }

        public DateTime? CreatedAt { get; set; }



    }
}
