namespace ConferenceFWebAPI.DTOs
{
    public class TopicDTO
    {
        public int TopicId { get; set; }
        public int ConferenceId { get; set; }
        public bool Status { get; set; } = true;
        public string TopicName { get; set; } = string.Empty;
    }
}
