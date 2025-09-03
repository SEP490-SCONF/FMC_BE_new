using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IEnumerable<UserConferenceRole>> GetUsersByConferenceIdAndRolesAsync(int conferenceId, List<int> roleIds)
        {
            return await _dao.GetUsersByConferenceIdAndRoles(conferenceId, roleIds);
        }
        public async Task<List<Conference>> GetConferencesByUserIdAndRoleAsync(int userId, string roleName)
        {
            return await _dao.GetConferencesByUserIdAndRoleAsync(userId, roleName);
        }

        public async Task<UserConferenceRole?> GetByUserAndConference(int userId, int conferenceId)
        {
            return await _dao.GetReviewerByUserAndConference(userId, conferenceId);
        }
        public async Task<bool> HasUserAnyRoleInConference(int userId, int conferenceId, List<string> roles)
        {
            return await _dao.HasUserAnyRoleInConference(userId, conferenceId, roles);
        }
        public Task<IEnumerable<UserConferenceRole>> GetByUserId(int userId)
        {
            return _dao.GetByUserId(userId);
        }
        public async Task<IEnumerable<User>> GetUsersWithoutAnyConferenceRole()
        {
            return await _dao.GetUsersWithoutAnyConferenceRole();
        }



    }
}
