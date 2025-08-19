using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IReviewRepository : IRepositoryBase<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByPaperId(int paperId);
        Task<int> GetReviewCount();
        Task<Review> GetByRevisionId(int revisionId);
        Task<Review> GetReviewByAssignmentId(int assignmentId);
        Task UpdatePaperAndRevisionStatus(int paperId, string paperStatus, int revisionId);
        Task<IEnumerable<Review>> GetCompletedReviewsByUserAndConference(int userId, int conferenceId);
        Task<int> CountCompletedReviewsByUserAndConference(int userId, int conferenceId);
    }
}
