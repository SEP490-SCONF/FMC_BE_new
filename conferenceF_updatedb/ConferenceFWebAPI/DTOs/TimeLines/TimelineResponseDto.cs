namespace ConferenceFWebAPI.DTOs.TimeLines
{
    public class TimelineResponseDto
    {
        public int TimeLineId { get; set; }
        public int ConferenceId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
