using System.ComponentModel.DataAnnotations;

namespace ConferenceFWebAPI.DTOs.UserConferenceRoles
{
    public class UserConferenceRoleChangeRoleDto
    {
        [Required(ErrorMessage = "User ID là bắt buộc.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Conference ID là bắt buộc.")]
        public int ConferenceId { get; set; }

        [Required(ErrorMessage = "New Conference Role ID là bắt buộc.")]
        public int NewConferenceRoleId { get; set; }
    }
}
