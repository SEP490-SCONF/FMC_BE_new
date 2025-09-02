namespace ConferenceFWebAPI.DTOs.ConferenceFees
{
    public class FeeDetailCreateDto
    {
        public int FeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Mode { get; set; } = "Regular";
        public string? Note { get; set; }
        public bool IsVisible { get; set; }

    }
}
