namespace ConferenceFWebAPI.DTOs.ConferenceFees
{
    public class FeeDetailOrganizerDto
    {
        public int FeeDetailId { get; set; }
        public int ConferenceId { get; set; }
        public int FeeTypeId { get; set; }
        public string FeeTypeName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Mode { get; set; } = "Regular";
        public string? Note { get; set; }
        public bool IsVisible { get; set; }
    }
}
