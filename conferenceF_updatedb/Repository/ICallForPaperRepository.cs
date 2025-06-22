using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICallForPaperRepository : IRepositoryBase<CallForPaper>
    {
        Task<IEnumerable<CallForPaper>> GetByConferenceId(int conferenceId);
    }
}
