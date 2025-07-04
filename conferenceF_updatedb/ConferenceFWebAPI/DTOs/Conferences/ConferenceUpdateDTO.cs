namespace ConferenceFWebAPI.DTOs.Conferences
{
    public class ConferenceUpdateDTO
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Location { get; set; }

        public string? CallForPaper { get; set; }

        // BannerImage là optional, chỉ cần gửi khi muốn thay đổi ảnh.
        public IFormFile? BannerImage { get; set; }
    }
}
