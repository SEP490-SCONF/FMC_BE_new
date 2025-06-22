using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewDAO _reviewDao;

        public ReviewRepository(ReviewDAO reviewDao)
        {
            _reviewDao = reviewDao;
        }

        public async Task<IEnumerable<Review>> GetAll()
        {
            return await _reviewDao.GetAllReviews();
        }

        public async Task<Review> GetById(int id)
        {
            return await _reviewDao.GetReviewById(id);
        }

        public async Task Add(Review entity)
        {
            await _reviewDao.AddReview(entity);
        }

        public async Task Update(Review entity)
        {
            await _reviewDao.UpdateReview(entity);
        }

        public async Task Delete(int id)
        {
            await _reviewDao.DeleteReview(id);
        }

        public async Task<IEnumerable<Review>> GetReviewsByPaperId(int paperId)
        {
            return await _reviewDao.GetReviewsByPaperId(paperId);
        }

        public async Task<int> GetReviewCount()
        {
            return await _reviewDao.GetReviewCount();
        }
    }
}
