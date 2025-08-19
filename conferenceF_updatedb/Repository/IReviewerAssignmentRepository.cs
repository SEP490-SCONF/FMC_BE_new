using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewerAssignmentRepository : IRepositoryBase<ReviewerAssignment>
    {
        Task<IEnumerable<ReviewerAssignment>> GetByPaperId(int paperId);
        Task<IEnumerable<ReviewerAssignment>> GetByReviewerId(int reviewerId);
        Task<IEnumerable<ReviewerAssignment>> GetAllByPaperId(int paperId);
        Task<List<ReviewerAssignment>> GetReviewersByPaperIdAsync(int paperId);
        Task<int> GetAssignedPaperCountByReviewerIdAndConferenceId(int reviewerId, int conferenceId);
        Task<int> GetAssignmentCountByReviewerIdAndConferenceId(int reviewerId, int conferenceId);

    }
}
