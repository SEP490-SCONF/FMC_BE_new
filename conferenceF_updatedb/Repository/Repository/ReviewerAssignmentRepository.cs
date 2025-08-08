using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ReviewerAssignmentRepository : IReviewerAssignmentRepository
    {
        private readonly ReviewerAssignmentDAO _dao;

        public ReviewerAssignmentRepository(ReviewerAssignmentDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<ReviewerAssignment> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(ReviewerAssignment entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(ReviewerAssignment entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetByPaperId(int paperId)
        {
            return await _dao.GetByPaperId(paperId);
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetByReviewerId(int reviewerId)
        {
            return await _dao.GetByReviewerId(reviewerId);
        }

        public async Task<IEnumerable<ReviewerAssignment>> GetAllByPaperId(int paperId)
        {
            return await _dao.GetAllByPaperId(paperId);
        }

    }
}
