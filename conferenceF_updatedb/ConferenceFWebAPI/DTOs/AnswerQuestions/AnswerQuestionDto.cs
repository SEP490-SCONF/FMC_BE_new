using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.AnswerQuestions
{
    public class AnswerQuestionDto
    {
        public int AnswerId { get; set; }
        public int FqId { get; set; }
        public int AnswerBy { get; set; }
        public int? ParentAnswerId { get; set; }
        public string Answer { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties
        public string? AnswererName { get; set; }
        public string? AnswererEmail { get; set; }
        public string? ForumQuestionTitle { get; set; }
        public int TotalLikes { get; set; }
        public bool HasReplies { get; set; }
        public int TotalReplies { get; set; }
        
        // Parent answer info if this is a reply
        public string? ParentAnswerText { get; set; }
        public string? ParentAnswererName { get; set; }
        
        // Current user like status
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class AnswerQuestionCreateDto
    {
        [Required]
        public int FqId { get; set; }

        [Required]
        public int AnswerBy { get; set; }

        public int? ParentAnswerId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Answer { get; set; } = null!;
    }

    public class AnswerQuestionUpdateDto
    {
        [Required]
        public int AnswerId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Answer { get; set; } = null!;
    }

    public class AnswerQuestionSummaryDto
    {
        public int AnswerId { get; set; }
        public string Answer { get; set; } = null!;
        public string? AnswererName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int TotalLikes { get; set; }
        public int TotalReplies { get; set; }
        public bool IsReply { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class PaginatedAnswerQuestionsDto
    {
        public List<AnswerQuestionDto> Answers { get; set; } = new List<AnswerQuestionDto>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string? SearchTerm { get; set; }
        public int ForumQuestionId { get; set; }
        public string? ForumQuestionTitle { get; set; }
    }

    public class AnswerLikeDto
    {
        public int LikeId { get; set; }
        public int AnswerId { get; set; }
        public int LikedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? LikerName { get; set; }
        public string? LikerEmail { get; set; }
        public string? AnswerContent { get; set; }
    }

    public class AnswerLikeCreateDto
    {
        [Required]
        public int AnswerId { get; set; }

        [Required]
        public int LikedBy { get; set; }
    }

    public class AnswerLikeToggleDto
    {
        [Required]
        public int AnswerId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class AnswerLikeStatsDto
    {
        public int AnswerId { get; set; }
        public int TotalLikes { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<AnswerLikeDto> RecentLikes { get; set; } = new List<AnswerLikeDto>();
    }
}
