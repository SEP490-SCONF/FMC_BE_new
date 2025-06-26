using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IConferenceRoleRepository : IRepositoryBase<ConferenceRole>
    {
        Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate);
    }
}
