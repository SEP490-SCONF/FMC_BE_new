namespace ConferenceFWebAPI.DTOs.UserProfile
{
    public class UpdateUserProfile
    {
        public string? Name { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
