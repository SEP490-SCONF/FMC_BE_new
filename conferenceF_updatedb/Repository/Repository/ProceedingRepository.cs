using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ProceedingRepository : IProceedingRepository
    {
        private readonly ProceedingDAO _dao;

        public ProceedingRepository(ConferenceFTestContext context)
        {
            _dao = new ProceedingDAO(context);
        }

        public async Task<Proceeding> CreateProceedingAsync(Proceeding proceeding)
        {
            return await _dao.CreateProceedingAsync(proceeding);
        }

        public async Task<Proceeding?> GetProceedingByIdAsync(int proceedingId)
        {
            return await _dao.GetProceedingByIdAsync(proceedingId);
        }
        public async Task<string?> GetFilePathByConferenceIdAsync(int conferenceId)
        {
            return await _dao.GetFilePathByConferenceIdAsync(conferenceId);
        }
        public async Task<Proceeding?> GetProceedingByConferenceIdAsync(int conferenceId)
        {
            return await _dao.GetProceedingByConferenceIdAsync(conferenceId);
        }

        public async Task UpdateProceedingAsync(Proceeding proceeding)
        {
            await _dao.UpdateProceedingAsync(proceeding);
        }

        public async Task<List<Paper>> GetPublishedPapersByConferenceAsync(int conferenceId)
        {
            return await _dao.GetPublishedPapersByConferenceAsync(conferenceId);
        }
    }
}
