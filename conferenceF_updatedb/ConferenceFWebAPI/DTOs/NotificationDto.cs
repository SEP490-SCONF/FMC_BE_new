namespace ConferenceFWebAPI.DTOs
{
    public class NotificationDto
    {
        public int NotiId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string RoleTarget { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
