using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduleDAO _dao;

        public ScheduleRepository(ScheduleDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<Schedule>> GetAll()
        {
            return await _dao.GetAllSchedules();
        }

        public async Task<Schedule> GetById(int id)
        {
            return await _dao.GetScheduleById(id);
        }

        public async Task Add(Schedule entity)
        {
            await _dao.AddSchedule(entity);
        }

        public async Task Update(Schedule entity)
        {
            await _dao.UpdateSchedule(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.DeleteSchedule(id);
        }

        public async Task<IEnumerable<Schedule>> GetByConferenceId(int conferenceId)
        {
            return await _dao.GetSchedulesByConference(conferenceId);
        }
    }
}
