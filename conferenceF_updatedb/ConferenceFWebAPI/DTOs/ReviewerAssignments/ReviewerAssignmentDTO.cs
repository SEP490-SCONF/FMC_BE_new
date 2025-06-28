using BussinessObject.Entity;

namespace ConferenceFWebAPI.DTOs.ReviewerAssignments
{
    public class ReviewerAssignmentDTO
    {
        public int AssignmentId { get; set; }

        public int PaperId { get; set; }

        public int ReviewerId { get; set; }

        public DateTime? AssignedAt { get; set; }


        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? TopicId { get; set; }
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmitDate { get; set; }
    }
}
