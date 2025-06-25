using BussinessObject.Entity;

namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }

        public int PaperId { get; set; }

        public int ReviewerId { get; set; }

        public int? RevisionId { get; set; }

        public int? Score { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public virtual BussinessObject.Entity.Paper Paper { get; set; } = null!;

        //public virtual User Reviewer { get; set; } = null!;
    }
}
