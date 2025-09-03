namespace BussinessObject.Entity
{
    public class FeeDetail
    {
        public int FeeDetailId { get; set; }
        public int ConferenceId { get; set; }
        public int FeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string? Mode { get; set; }
        public string? Note { get; set; }
        public bool IsVisible { get; set; } = false;
        public Conference Conference { get; set; } = null!;
        public FeeType FeeType { get; set; } = null!;
    }
}