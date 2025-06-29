using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
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
        public async Task<Review> GetByRevisionId(int revisionId)
        {
            return await _reviewDao.GetByRevisionId(revisionId);
        }
        public async Task<Review> GetReviewByAssignmentId(int assignmentId)
        {
            return await _reviewDao.GetReviewByAssignmentId(assignmentId);
        }
        public async Task UpdatePaperAndRevisionStatus(int paperId, string paperStatus, int revisionId)
        {
            await _reviewDao.UpdatePaperAndRevisionStatus(paperId, paperStatus, revisionId);
        }

    }
}
