using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Repository.Repository
{
    public class FeeTypeRepository : IFeeTypeRepository
    {
        private readonly FeeTypeDAO _dao;

        public FeeTypeRepository(FeeTypeDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<FeeType>> GetAll() => await _dao.GetAll();
        public async Task<FeeType> GetById(int id) => await _dao.GetById(id);
        public async Task Add(FeeType entity) => await _dao.Add(entity);
        public async Task Update(FeeType entity) => await _dao.Update(entity);
        public async Task Delete(int id) => await _dao.Delete(id);
        public IQueryable<FeeType> GetAllQueryable() => _dao.GetAllQueryable();
    }
}