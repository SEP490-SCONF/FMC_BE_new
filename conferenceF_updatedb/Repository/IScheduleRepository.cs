using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IScheduleRepository 
    {
        // Thêm lịch
        Task<Schedule> AddScheduleAsync(Schedule schedule);

        // Lấy lịch trình theo ID
        Task<Schedule?> GetScheduleByIdAsync(int scheduleId);

        // Lấy tất cả lịch trình của một hội thảo
        Task<List<Schedule>> GetSchedulesByConferenceIdAsync(int conferenceId);

        // Cập nhật lịch
        Task UpdateScheduleAsync(Schedule schedule);

        // Xóa lịch
        Task DeleteScheduleAsync(int scheduleId);
        Task<List<Schedule>> GetSchedulesByTimelineIdAsync(int timelineId);

    }
}
