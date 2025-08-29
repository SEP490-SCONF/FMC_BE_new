namespace ConferenceFWebAPI.DTOs.ConferenceFees
{
    public class FeeDetailPublicDto
    {
        public int FeeDetailId { get; set; }
        public int FeeTypeId { get; set; }
        public string FeeTypeName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Mode { get; set; } = "Regular";
        public bool IsVisible { get; set; }
    }
}
