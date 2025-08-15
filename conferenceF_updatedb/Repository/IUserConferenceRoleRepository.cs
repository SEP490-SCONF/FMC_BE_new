using BussinessObject.Entity;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUserConferenceRoleRepository : IRepositoryBase<UserConferenceRole>
    {
        Task<IEnumerable<UserConferenceRole>> GetByConferenceId(int conferenceId);
        Task<bool> IsReviewer(int userId);
        Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate);
        Task<UserConferenceRole?> UpdateConferenceRoleForUserInConference(int userId, int conferenceId, int newConferenceRoleId);
        Task<IEnumerable<UserConferenceRole>> GetUsersByConferenceIdAndRolesAsync(int conferenceId, List<int> roleIds);
        Task<List<Conference>> GetConferencesByUserIdAndRoleAsync(int userId, string roleName);
        Task<UserConferenceRole?> GetByUserAndConference(int userId, int conferenceId);
        Task<bool> HasUserAnyRoleInConference(int userId, int conferenceId, List<string> roles);
    }
}

