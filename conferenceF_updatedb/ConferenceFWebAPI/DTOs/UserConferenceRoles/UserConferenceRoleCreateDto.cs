namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class UserConferenceRoleCreateDto
    {
        public int UserId { get; set; } // Thay UserId bằng Email
        public int ConferenceRoleId { get; set; } 

        
        public int ConferenceId { get; set; } // Giữ lại ConferenceId
    }
}
