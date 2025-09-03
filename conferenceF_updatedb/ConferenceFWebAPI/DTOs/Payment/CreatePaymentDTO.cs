namespace ConferenceFWebAPI.DTOs.Payment
{
    //public class CreatePaymentDTO
    //{
    //    public int UserId { get; set; }
    //    public int ConferenceId { get; set; }
    //    public int? PaperId { get; set; }
    //    public decimal Amount { get; set; }
    //    public string? Currency { get; set; }
    //    public long OrderCode { get; set; }
    //    public string? Purpose { get; set; }
    //}

    public class CreatePaymentDTO
    {
        public int ConferenceId { get; set; }
        public int? PaperId { get; set; }
        public List<FeeItemDTO> Fees { get; set; } = new();
    }

    public class FeeItemDTO
    {
        public int FeeDetailId { get; set; }
        public int Quantity { get; set; } = 1;
    }

}
