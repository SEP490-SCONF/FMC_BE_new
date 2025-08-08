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
}
