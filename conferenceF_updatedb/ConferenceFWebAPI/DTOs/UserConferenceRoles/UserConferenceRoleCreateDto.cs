namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class UserConferenceRoleCreateDto
    {
        public int UserId { get; set; }
        public int ConferenceRoleId { get; set; }
        public int ConferenceId { get; set; }

    }
}
