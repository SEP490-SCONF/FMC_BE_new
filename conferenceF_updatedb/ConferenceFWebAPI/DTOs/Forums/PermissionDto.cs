using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.Forums
{
    public class PermissionCheckDto
    {
        public bool HasPermission { get; set; }
        public int? UserId { get; set; }
        public int ConferenceId { get; set; }
        public string? ConferenceTitle { get; set; }
        public string? UserRole { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CheckedAt { get; set; }
    }

    public class ForumModerationPermissionDto
    {
        public bool CanModerate { get; set; }
        public bool CanDeleteComments { get; set; }
        public bool CanDeleteQuestions { get; set; }
        public bool CanDeleteAnswers { get; set; }
        public bool CanBanUsers { get; set; }
        public string PermissionLevel { get; set; } = null!;
        public DateTime CheckedAt { get; set; }
    }
}
