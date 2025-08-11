namespace ConferenceFWebAPI.DTOs.Payment
{
    public class UpdatePaymentDTO
    {
        public string? PayStatus { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal? Amount { get; set; }
    }
}
