namespace ConferenceFWebAPI.DTOs.UserProfile
{
    public class UserProfile
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int RoleId { get; set; }
    }
}
