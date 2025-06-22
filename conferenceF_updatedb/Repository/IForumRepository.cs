using BussinessObject.Entity;
using System.Threading.Tasks;

namespace Repository
{
    public interface IForumRepository : IRepositoryBase<Forum>
    {
        Task<Forum> GetByConferenceId(int conferenceId);
    }
}
