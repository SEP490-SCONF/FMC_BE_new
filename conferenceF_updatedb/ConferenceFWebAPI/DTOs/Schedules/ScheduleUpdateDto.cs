namespace ConferenceFWebAPI.DTOs.Schedules
{
    public class ScheduleUpdateDto
    {
        public string? SessionTitle { get; set; }
        public string? Location { get; set; }
        public DateTime? PresentationStartTime { get; set; }
        public DateTime? PresentationEndTime { get; set; }
    }
}
