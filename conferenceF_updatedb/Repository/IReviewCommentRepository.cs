using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewCommentRepository : IRepositoryBase<ReviewComment>
    {
        Task<IEnumerable<ReviewComment>> GetByReviewId(int reviewId);
        Task<ReviewComment> GetByHighlightId(int highlightId);
       

    }
}
