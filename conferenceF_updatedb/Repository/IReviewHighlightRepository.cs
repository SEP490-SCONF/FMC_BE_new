using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewHighlightRepository : IRepositoryBase<ReviewHighlight>
    {
        Task<IEnumerable<ReviewHighlight>> GetByReviewId(int reviewId);
    }
}
