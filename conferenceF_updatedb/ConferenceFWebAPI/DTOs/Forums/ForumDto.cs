using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Forums
{
    public class ForumDto
    {
        public int ForumId { get; set; }
        public int ConferenceId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties info
        public string? ConferenceTitle { get; set; }
        public int TotalQuestions { get; set; }
    }
    
    public class ForumCreateDto
    {
        [Required]
        public int ConferenceId { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = null!;
    }
    
    public class ForumUpdateDto
    {
        [Required]
        public int ForumId { get; set; }
        
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = null!;
    }
}
