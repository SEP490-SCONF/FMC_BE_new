using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaperRepository
    {
        Task<List<Paper>> GetAllAsync();
        Task<Paper?> GetByIdAsync(int id);
        Task AddAsync(Paper paper, List<int> authorIds);
        Task UpdateAsync(Paper paper);
        Task DeleteAsync(int id);
    }
}
