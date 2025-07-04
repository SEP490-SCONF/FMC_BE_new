using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ReviewCommentRepository : IReviewCommentRepository
    {
        private readonly ReviewCommentDAO _dao;

        public ReviewCommentRepository(ReviewCommentDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<ReviewComment>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<ReviewComment> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(ReviewComment entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(ReviewComment entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<ReviewComment>> GetByReviewId(int reviewId)
        {
            return await _dao.GetByReviewId(reviewId);
        }
        public async Task<ReviewComment> GetByHighlightId(int highlightId)
        {
            return await _dao.GetByHighlightId(highlightId);
        }
    }
}
