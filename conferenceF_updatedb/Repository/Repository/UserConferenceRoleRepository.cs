using BussinessObject.Entity;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public Task<IEnumerable<UserConferenceRole>> GetAll() => _dao.GetAll();

        public Task<UserConferenceRole> GetById(int id) => _dao.GetById(id);

        public Task<IEnumerable<UserConferenceRole>> GetByConferenceId(int conferenceId)
            => _dao.GetByConferenceId(conferenceId);

        public Task Add(UserConferenceRole role) => _dao.Add(role);

        public Task Update(UserConferenceRole role) => _dao.Update(role);

        public Task Delete(int id) => _dao.Delete(id);
        public async Task<bool> IsReviewer(int userId)
        {
            return await _dao.IsReviewer(userId);
        }

        public async Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate)
        {
            return await _dao.GetByCondition(predicate);
        }
        public async Task<UserConferenceRole?> UpdateConferenceRoleForUserInConference(int userId, int conferenceId, int newConferenceRoleId)
        {
            // Repository chỉ ủy quyền công việc cho DAO
            return await _dao.UpdateConferenceRoleForUser(userId, conferenceId, newConferenceRoleId);
        }
    }
}
