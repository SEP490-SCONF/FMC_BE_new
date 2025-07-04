namespace ConferenceFWebAPI.DTOs.UserProfile
{
    public class UserInformationDTO
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        //public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool? Status { get; set; }

    }
}
