using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICallForPaperRepository 
    {
        Task<IEnumerable<CallForPaper>> GetAllCallForPapers();
        Task<CallForPaper?> GetCallForPaperById(int id);
        Task<IEnumerable<CallForPaper>> GetCallForPapersByConferenceId(int conferenceId); // NEW
        Task AddCallForPaper(CallForPaper callForPaper);
        Task UpdateCallForPaper(CallForPaper callForPaper);
        Task DeleteCallForPaper(int id);
        Task<bool> HasActiveCallForPaper(int conferenceId);

    }
}
