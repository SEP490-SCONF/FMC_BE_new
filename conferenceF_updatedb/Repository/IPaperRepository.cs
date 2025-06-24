using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaperRepository
    {
        Task<Paper> GetPaperByIdAsync(int paperId);
        Task AddPaperAsync(Paper paper);
        Task UpdatePaperAsync(Paper paper);
        Task DeletePaperAsync(int paperId);
        // Có thể thêm các phương thức khác tùy theo nhu cầu của ứng dụng
        // Ví dụ: Task<IEnumerable<Paper>> GetAllPapersAsync();
    }
}
