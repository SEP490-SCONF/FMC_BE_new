namespace ConferenceFWebAPI.DTOs.TimeLines
{
    public class TimelineReminderDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
    }
}
