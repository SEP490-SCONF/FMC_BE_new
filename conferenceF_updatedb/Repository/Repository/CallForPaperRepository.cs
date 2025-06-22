using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class CallForPaperRepository : ICallForPaperRepository
    {
        private readonly CallForPaperDAO _dao;

        public CallForPaperRepository(CallForPaperDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<CallForPaper>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<CallForPaper> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(CallForPaper entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(CallForPaper entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<CallForPaper>> GetByConferenceId(int conferenceId)
        {
            return await _dao.GetByConferenceId(conferenceId);
        }
    }
}
