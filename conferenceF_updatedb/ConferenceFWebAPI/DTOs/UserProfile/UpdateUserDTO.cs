namespace ConferenceFWebAPI.DTOs.UserProfile
{
    public class UpdateUserDTO
    {
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public int? RoleId { get; set; }
    }
}
