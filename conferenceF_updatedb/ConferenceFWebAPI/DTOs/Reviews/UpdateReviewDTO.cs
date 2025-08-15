namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class UpdateReviewDTO
    {
        public string? Comments { get; set; }
        public int? Score { get; set; }
        public string PaperStatus { get; set; }

    }
}
