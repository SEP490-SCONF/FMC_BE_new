using BussinessObject.Entity;

namespace ConferenceFWebAPI.DTOs.Schedules
{
    public class ScheduleResponseDto
    {
        public int ScheduleId { get; set; }
        public string? SessionTitle { get; set; }
        public string? Location { get; set; }
        public DateTime? PresentationStartTime { get; set; }
        public DateTime? PresentationEndTime { get; set; }


        // Lấy nguyên đối tượng Paper
        public Paper? Paper { get; set; }
        public TimeLine? Timeline { get; set; }


        // Lấy nguyên đối tượng Conference
        public Conference? Conference { get; set; }

        // Presenter info
        public int PresenterId { get; set; }
        public string? PresenterName { get; set; }

        public int? PaperScore { get; set; }

    }
}
