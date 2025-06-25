namespace ConferenceFWebAPI.DTOs
{
    public class AddOrUpdateTopicDTO
    {
        public bool? Status { get; set; }
        public string TopicName { get; set; } = null!;

    }
    public class TopicDTO
    {
        public int TopicId { get; set; }

        public bool? Status { get; set; }

        public string TopicName { get; set; } = null!;

    }
}
