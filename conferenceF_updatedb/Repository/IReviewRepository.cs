using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewRepository : IRepositoryBase<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByPaperId(int paperId);
        Task<int> GetReviewCount();
    }
}
