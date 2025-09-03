namespace BussinessObject.Entity
{
    public class FeeType
    {
        public int FeeTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<FeeDetail> FeeDetails { get; set; } = new List<FeeDetail>();
    }
}