using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaperRepository
    {
        Task<Paper?> GetPaperWithConferenceAndTimelinesAsync(int paperId);

        IQueryable<Paper> GetAllPapers();
        Task<Paper?> GetPaperByIdAsync(int paperId);
        Task<Paper?> GetPaperByIdWithIncludesAsync(int paperId);
        Task AddPaperAsync(Paper paper);
        Task UpdatePaperAsync(Paper paper);
        Task DeletePaperAsync(int paperId);
        List<Paper> GetPapersByConferenceId(int conferenceId);
        List<Paper> GetPapersByUserIdAndConferenceId(int userId, int conferenceId);
        List<Paper> GetPapersByConferenceIdAndStatus(int conferenceId, string status);
        List<Paper> GetPublishedPapersByConferenceId(int conferenceId);
        Task<List<Paper>> GetAcceptedPapersWithRegistrationsByAuthor(int authorId);
        Task<List<PaperAuthor>> GetAuthorsByPaperIdAsync(int paperId);
        Task<Paper> GetPaperWithAuthorsAsync(int paperId);
        List<Paper> GetPapersByUserId(int userId);
        List<Paper> GetPresentedPapersByConferenceId(int conferenceId);

        Task<List<Paper>> GetPapersByIdsAsync(List<int> paperIds);
        List<Paper> GetAcceptedPapersByUserIdAndConferenceId(int userId, int conferenceId);
        Task<int> GetPdfPageCountByPaperIdAsync(int paperId);






        // Có thể thêm các phương thức khác tùy theo nhu cầu của ứng dụng
        // Ví dụ: Task<IEnumerable<Paper>> GetAllPapersAsync();
    }
}
