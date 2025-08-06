namespace ConferenceFWebAPI.DTOs.Reviews
{
    public class HighlightAreaDTO
    {
        public int HighlightAreaId { get; set; }
        public int? PageIndex { get; set; }
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
    }
}
