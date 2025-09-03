namespace ConferenceFWebAPI.DTOs.ConferenceFees
{
    public class FeeTypeDto
    {
        public int FeeTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
