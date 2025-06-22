using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly RegistrationDAO _registrationDao;

        public RegistrationRepository(RegistrationDAO registrationDao)
        {
            _registrationDao = registrationDao;
        }

        public async Task<IEnumerable<Registration>> GetAll()
        {
            return await _registrationDao.GetAll();
        }

        public async Task<Registration> GetById(int id)
        {
            return await _registrationDao.GetById(id);
        }

        public async Task Add(Registration entity)
        {
            await _registrationDao.Add(entity);
        }

        public async Task Update(Registration entity)
        {
            await _registrationDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _registrationDao.Delete(id);
        }

        public async Task<IEnumerable<Registration>> GetByConferenceId(int conferenceId)
        {
            return await _registrationDao.GetByConferenceId(conferenceId);
        }

        public async Task<IEnumerable<Registration>> GetByUserId(int userId)
        {
            return await _registrationDao.GetByUserId(userId);
        }
    }
}
