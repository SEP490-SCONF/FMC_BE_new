using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class HighlightAreaRepository : IHighlightAreaRepository
    {
        private readonly HighlightAreaDAO _dao;

        public HighlightAreaRepository(HighlightAreaDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<HighlightArea>> GetAllAsync()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<HighlightArea> GetByIdAsync(int id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public async Task<IEnumerable<HighlightArea>> GetByHighlightIdAsync(int highlightId)
        {
            return await _dao.GetByHighlightIdAsync(highlightId);
        }

        public async Task AddAsync(HighlightArea entity)
        {
            await _dao.AddAsync(entity);
        }

        public async Task UpdateAsync(HighlightArea entity)
        {
            await _dao.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _dao.DeleteAsync(id);
        }
    }
}
