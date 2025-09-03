using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IFeeDetailRepository
    {
        Task<IEnumerable<FeeDetail>> GetAll();
        Task<FeeDetail> GetById(int id);
        Task Add(FeeDetail entity);
        Task Update(FeeDetail entity);
        Task Delete(int id);
        IQueryable<FeeDetail> GetAllQueryable();
        Task<IEnumerable<FeeDetail>> GetByConferenceId(int conferenceId);
    }
}