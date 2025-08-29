namespace ConferenceFWebAPI.DTOs.ConferenceFees
{
    public class FeeDetailUpdateDto
    {
        public decimal Amount { get; set; }
        public string? Mode { get; set; }
        public string? Note { get; set; }
        public bool IsVisible { get; set; }
    }

}
