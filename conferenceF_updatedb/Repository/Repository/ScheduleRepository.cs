using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduleDAO _dao;

        public ScheduleRepository(ConferenceFTestContext context)
        {
            _dao = new ScheduleDAO(context);
        }

        public async Task<Schedule> AddScheduleAsync(Schedule schedule)
        {
            return await _dao.AddScheduleAsync(schedule);
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _dao.GetScheduleByIdAsync(scheduleId);
        }

        public async Task<List<Schedule>> GetSchedulesByConferenceIdAsync(int conferenceId)
        {
            return await _dao.GetSchedulesByConferenceIdAsync(conferenceId);
        }

        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            await _dao.UpdateScheduleAsync(schedule);
        }

        public async Task DeleteScheduleAsync(int scheduleId)
        {
            await _dao.DeleteScheduleAsync(scheduleId);
        }
    }
}
