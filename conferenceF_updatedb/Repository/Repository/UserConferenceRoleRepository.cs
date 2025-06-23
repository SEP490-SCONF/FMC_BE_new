using BussinessObject.Entity;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class UserConferenceRoleRepository : IUserConferenceRoleRepository
    {
        private readonly UserConferenceRoleDAO _dao;

        public UserConferenceRoleRepository(UserConferenceRoleDAO dao)
        {
            _dao = dao;
        }

        public Task<IEnumerable<UserConferenceRole>> GetAllAsync() => _dao.GetAll();

        public Task<UserConferenceRole> GetByIdAsync(int id) => _dao.GetById(id);

        public Task<IEnumerable<UserConferenceRole>> GetByConferenceIdAsync(int conferenceId)
            => _dao.GetByConferenceId(conferenceId);

        public Task AddAsync(UserConferenceRole role) => _dao.Add(role);

        public Task UpdateAsync(UserConferenceRole role) => _dao.Update(role);

        public Task DeleteAsync(int id) => _dao.Delete(id);
        public async Task<bool> IsReviewer(int userId)
        {
            return await _dao.IsReviewer(userId);
        }
    }
}
