using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IScheduleRepository : IRepositoryBase<Schedule>
    {
        Task<IEnumerable<Schedule>> GetByConferenceId(int conferenceId);
    }
}
