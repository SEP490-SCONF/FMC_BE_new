namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class UserConferenceRoleViewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public int ConferenceRoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public int ConferenceId { get; set; }
        public string ConferenceTitle { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        public DateTime? AssignedAt { get; set; }
    }
}
