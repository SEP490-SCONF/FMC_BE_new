namespace ConferenceFWebAPI.DTOs
{
    public class AddOrUpdateTopicDTO
    {
        public string TopicName { get; set; } = null!;
        public bool? Status { get; set; }

    }
    public class TopicDTO
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = null!;
        public bool? Status { get; set; }

    }
}
