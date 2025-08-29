using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IFeeTypeRepository
    {
        Task<IEnumerable<FeeType>> GetAll();
        Task<FeeType> GetById(int id);
        Task Add(FeeType entity);
        Task Update(FeeType entity);
        Task Delete(int id);
        IQueryable<FeeType> GetAllQueryable();
    }
}