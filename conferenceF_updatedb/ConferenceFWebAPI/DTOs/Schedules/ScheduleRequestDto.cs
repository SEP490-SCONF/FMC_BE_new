namespace ConferenceFWebAPI.DTOs.Schedules
{
    public class ScheduleRequestDto
    {
        public int TimelineId { get; set; }

        public int? ConferenceId { get; set; }
        public int? PaperId { get; set; }
        public int? PresenterId { get; set; }
        public string? SessionTitle { get; set; }
        public string? Location { get; set; }
        public DateTime? PresentationStartTime { get; set; }
        public DateTime? PresentationEndTime { get; set; }
    }
}
