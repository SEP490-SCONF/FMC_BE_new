using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ProceedingRepository : IProceedingRepository
    {
        private readonly ProceedingDAO _proceedingDao;

        public ProceedingRepository(ProceedingDAO proceedingDao)
        {
            _proceedingDao = proceedingDao;
        }

        public async Task<IEnumerable<Proceeding>> GetAll()
        {
            return await _proceedingDao.GetAll();
        }

        public async Task<Proceeding> GetById(int id)
        {
            return await _proceedingDao.GetById(id);
        }

        public async Task Add(Proceeding entity)
        {
            await _proceedingDao.Add(entity);
        }

        public async Task Update(Proceeding entity)
        {
            await _proceedingDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _proceedingDao.Delete(id);
        }

        public async Task<IEnumerable<Proceeding>> GetByConferenceId(int conferenceId)
        {
            return await _proceedingDao.GetByConferenceId(conferenceId);
        }
    }
}
