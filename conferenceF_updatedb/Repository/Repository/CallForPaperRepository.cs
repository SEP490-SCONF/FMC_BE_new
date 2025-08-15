using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class CallForPaperRepository : ICallForPaperRepository
    {
        private readonly CallForPaperDAO _callForPaperDAO;

        public CallForPaperRepository(CallForPaperDAO callForPaperDAO)
        {
            _callForPaperDAO = callForPaperDAO;
        }

        public async Task<IEnumerable<CallForPaper>> GetAllCallForPapers()
        {
            return await _callForPaperDAO.GetAllCallForPapers();
        }

        public async Task<CallForPaper?> GetCallForPaperById(int id)
        {
            return await _callForPaperDAO.GetCallForPaperById(id);
        }

        public async Task<IEnumerable<CallForPaper>> GetCallForPapersByConferenceId(int conferenceId) // NEW
        {
            return await _callForPaperDAO.GetCallForPapersByConferenceId(conferenceId);
        }

        public async Task AddCallForPaper(CallForPaper callForPaper)
        {
            await _callForPaperDAO.AddCallForPaper(callForPaper);
        }

        public async Task UpdateCallForPaper(CallForPaper callForPaper)
        {
            await _callForPaperDAO.UpdateCallForPaper(callForPaper);
        }

        public async Task DeleteCallForPaper(int id)
        {
            await _callForPaperDAO.DeleteCallForPaper(id);
        }

        public async Task<bool> HasActiveCallForPaper(int conferenceId)
        {
            return await _callForPaperDAO.HasActiveCallForPaper(conferenceId);
        }

    }
}
