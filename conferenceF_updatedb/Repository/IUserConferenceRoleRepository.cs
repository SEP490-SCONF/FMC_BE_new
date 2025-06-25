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
        Task<IEnumerable<UserConferenceRole>> GetByConferenceIdAsync(int conferenceId);
        Task<bool> IsReviewer(int userId);
        Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate);
    }
}
﻿
