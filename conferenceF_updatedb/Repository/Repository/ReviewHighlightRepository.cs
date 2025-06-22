using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ReviewHighlightRepository : IReviewHighlightRepository
    {
        private readonly ReviewHighlightDAO _dao;

        public ReviewHighlightRepository(ReviewHighlightDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<ReviewHighlight>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<ReviewHighlight> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(ReviewHighlight entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(ReviewHighlight entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<ReviewHighlight>> GetByReviewId(int reviewId)
        {
            return await _dao.GetByReviewId(reviewId);
        }
    }
}
