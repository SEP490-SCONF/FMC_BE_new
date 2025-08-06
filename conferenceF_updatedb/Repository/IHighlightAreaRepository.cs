using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IHighlightAreaRepository
    {
        Task<IEnumerable<HighlightArea>> GetAllAsync();
        Task<HighlightArea> GetByIdAsync(int id);
        Task<IEnumerable<HighlightArea>> GetByHighlightIdAsync(int highlightId);
        Task AddAsync(HighlightArea entity);
        Task UpdateAsync(HighlightArea entity);
        Task DeleteAsync(int id);
    }
}
