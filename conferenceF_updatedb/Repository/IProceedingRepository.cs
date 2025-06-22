using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IProceedingRepository : IRepositoryBase<Proceeding>
    {
        Task<IEnumerable<Proceeding>> GetByConferenceId(int conferenceId);
    }
}
