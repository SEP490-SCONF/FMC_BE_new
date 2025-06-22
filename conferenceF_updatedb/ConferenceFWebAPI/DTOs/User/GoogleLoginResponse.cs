namespace ConferenceFWebAPI.DTOs.User
{
    public class GoogleLoginResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; }
    }
}
