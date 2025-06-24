namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class AddReviewDTO
    {
        public int PaperId { get; set; }
        public int ReviewerId { get; set; }
        public string? Comments { get; set; }
    }
}
