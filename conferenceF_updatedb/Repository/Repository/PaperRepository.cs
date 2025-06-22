using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PaperRepository : IPaperRepository
    {
        private readonly PaperDAO _dao;

        public PaperRepository(PaperDAO dao)
        {
            _dao = dao;
        }

        public Task<List<Paper>> GetAllAsync() => _dao.GetAllAsync();
        public Task<Paper?> GetByIdAsync(int id) => _dao.GetByIdAsync(id);
        public Task AddAsync(Paper paper, List<int> authorIds)
        {
            return _dao.AddAsync(paper, authorIds);
        }
        public Task UpdateAsync(Paper paper) => _dao.UpdateSimpleAsync(paper);
        public Task DeleteAsync(int id) => _dao.DeleteAsync(id);
    }
}
