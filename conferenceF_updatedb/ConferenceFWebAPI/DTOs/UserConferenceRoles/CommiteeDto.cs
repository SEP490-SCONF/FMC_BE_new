namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class CommiteeDto
    {
        public class CommitteeViewDto
        {
            public int ConferenceId { get; set; }
            public string ConferenceTitle { get; set; } = null!;
            public List<CommitteeGroupDto> Groups { get; set; } = new();
        }

        public class CommitteeGroupDto
        {
            public string GroupName { get; set; } = null!;
            public List<CommitteeMemberDto> Members { get; set; } = new();
        }

        public class CommitteeMemberDto
        {
            public int UserId { get; set; }
            public string DisplayName { get; set; } = null!;
            public string? Email { get; set; }
            public string? AvatarUrl { get; set; }
            public string? RoleName { get; set; }
            public string? SpecificTitle { get; set; }
            public string? Affiliation { get; set; }
            public string? Expertise { get; set; }
            public DateTime? ConfirmedAt { get; set; }
        }
    }
}
