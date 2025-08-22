namespace ConferenceFWebAPI.DTOs.Payment
{
    public class PaymentDTO
    {
        public int PayId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int ConferenceId { get; set; }
        public string? ConferenceName { get; set; }
        public int? RegId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? PayStatus { get; set; }
        public string? PayOsOrderCode { get; set; }
        public string? PayOsCheckoutUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? PaperId { get; set; }
        public string? Purpose { get; set; }
    }
}
