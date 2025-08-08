namespace ConferenceFWebAPI.DTOs.Payment
{
    public class CreatePaymentDTO
    {
        public int UserId { get; set; }
        public int ConferenceId { get; set; }
        public int? PaperId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Purpose { get; set; }
    }
}
