namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class UserConferenceRoleCreateDto
    {
        public string Email { get; set; } // Thay UserId bằng Email
        public int ConferenceId { get; set; } // Giữ lại ConferenceId
    }
}
