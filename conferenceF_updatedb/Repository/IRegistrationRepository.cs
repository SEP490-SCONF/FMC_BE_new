using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IRegistrationRepository : IRepositoryBase<Registration>
    {
        Task<IEnumerable<Registration>> GetByConferenceId(int conferenceId);
        Task<IEnumerable<Registration>> GetByUserId(int userId);
    }
}
