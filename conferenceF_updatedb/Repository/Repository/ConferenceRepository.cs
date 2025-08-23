using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;


namespace Repository
{
    public class ConferenceRepository : IConferenceRepository
    {
        private readonly ConferenceDAO _conferenceDao;

        
        public ConferenceRepository(ConferenceDAO conferenceDao)
        {
            _conferenceDao = conferenceDao;
        }
        public async Task<IEnumerable<Conference>> GetAll()
        {
            return await _conferenceDao.GetAllConferences();
        }

        public async Task<IEnumerable<Conference>> GetAllConferencesFalse()
        {
            return await _conferenceDao.GetAllConferences();
        }

        public async Task<Conference> GetById(int id)
        {
            return await _conferenceDao.GetConferenceById(id);
        }

        public async Task Add(Conference entity)
        {
            await _conferenceDao.AddConference(entity);
        }

        public async Task Update(Conference entity)
        {
            await _conferenceDao.UpdateConference(entity);
        }
        public async Task UpdateConferenceStatus(int conferenceId, bool newStatus)
        {
            await _conferenceDao.UpdateConferenceStatus(conferenceId, newStatus);
        }

        public async Task Delete(int id)
        {
            await _conferenceDao.DeleteConference(id);
        }

        public async Task<int> GetConferenceCount()
        {
            return await _conferenceDao.GetConferenceCount();
        }
        public IQueryable<Conference> GetAllQueryable()
        {
            return _conferenceDao.GetAllQueryable();
        }

        public Task<Conference> Insert(Conference conference)
        {
            return _conferenceDao.InsertConference(conference);
        }
    }
}
