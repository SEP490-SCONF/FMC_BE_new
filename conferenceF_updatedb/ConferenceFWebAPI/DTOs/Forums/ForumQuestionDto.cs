using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Forums
{
    public class ForumQuestionDto
    {
        public int FqId { get; set; }
        public int AskBy { get; set; }
        public int ForumId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Question { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties info
        public string? AskerName { get; set; }
        public string? AskerEmail { get; set; }
        public string? ForumTitle { get; set; }
        public int TotalAnswers { get; set; }
        public int TotalLikes { get; set; }
        
        // Current user like status
        public bool IsLikedByCurrentUser { get; set; }
    }
    
    public class ForumQuestionCreateDto
    {
        [Required]
        public int AskBy { get; set; }
        
        [Required]
        public int ForumId { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = null!;
        
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = null!;
        
        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Question { get; set; } = null!;
    }
    
    public class ForumQuestionUpdateDto
    {
        [Required]
        public int FqId { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = null!;
        
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = null!;
        
        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Question { get; set; } = null!;
    }
    
    public class ForumQuestionSummaryDto
    {
        public int FqId { get; set; }
        public string Title { get; set; } = null!;
        public string? AskerName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int TotalAnswers { get; set; }
        public int TotalLikes { get; set; }
    }

    public class ForumQuestionWithAnswersDto
    {
        public int FqId { get; set; }
        public int AskBy { get; set; }
        public int ForumId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Question { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string AskerName { get; set; } = null!;
        public string? AskerEmail { get; set; }
        public int TotalAnswers { get; set; }
        public int TotalLikes { get; set; }
        public List<AnswerQuestionDto> RecentAnswers { get; set; } = new List<AnswerQuestionDto>();
        
        // Current user like status
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class AnswerQuestionDto
    {
        public int AnswerId { get; set; }
        public int AnswerBy { get; set; }
        public string Answer { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string AnswererName { get; set; } = null!;
        public string? AnswererEmail { get; set; }
        public int? ParentAnswerId { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class PaginatedForumQuestionsDto
    {
        public List<ForumQuestionWithAnswersDto> Questions { get; set; } = new List<ForumQuestionWithAnswersDto>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class QuestionLikeDto
    {
        public int LikeId { get; set; }
        public int FqId { get; set; }
        public int LikedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? LikerName { get; set; }
        public string? LikerEmail { get; set; }
        public string? QuestionTitle { get; set; }
    }

    public class QuestionLikeToggleDto
    {
        [Required]
        public int FqId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class QuestionLikeStatsDto
    {
        public int FqId { get; set; }
        public int TotalLikes { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<QuestionLikeDto> RecentLikes { get; set; } = new List<QuestionLikeDto>();
    }
}
