namespace ConferenceFWebAPI.DTOs.ReviewHightlights
{
    public class AddReviewHightlightDTO
    {

        public int ReviewId { get; set; }

        public int? PageNumber { get; set; }

        public int? OffsetStart { get; set; }

        public int? OffsetEnd { get; set; }

        public string? TextHighlighted { get; set; }

    }
}
