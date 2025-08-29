using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Repository.Repository
{
    public class FeeDetailRepository : IFeeDetailRepository
    {
        private readonly FeeDetailDAO _dao;

        public FeeDetailRepository(FeeDetailDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<FeeDetail>> GetAll() => await _dao.GetAll();
        public async Task<FeeDetail> GetById(int id) => await _dao.GetById(id);
        public async Task Add(FeeDetail entity) => await _dao.Add(entity);
        public async Task Update(FeeDetail entity) => await _dao.Update(entity);
        public async Task Delete(int id) => await _dao.Delete(id);
        public IQueryable<FeeDetail> GetAllQueryable() => _dao.GetAllQueryable();
        public async Task<IEnumerable<FeeDetail>> GetByConferenceId(int conferenceId) => await _dao.GetByConferenceId(conferenceId);
    }
}