<<<<<<< HEAD
﻿using BussinessObject.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUserConferenceRoleRepository
    {
        Task<IEnumerable<UserConferenceRole>> GetAllAsync();
        Task<UserConferenceRole> GetByIdAsync(int id);
        Task<IEnumerable<UserConferenceRole>> GetByConferenceIdAsync(int conferenceId);
        Task AddAsync(UserConferenceRole role);
        Task UpdateAsync(UserConferenceRole role);
        Task DeleteAsync(int id);
        Task<bool> IsReviewer(int userId);

    }
}
=======
﻿using BussinessObject.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUserConferenceRoleRepository
    {
        Task<IEnumerable<UserConferenceRole>> GetAllAsync();
        Task<UserConferenceRole> GetByIdAsync(int id);
        Task<IEnumerable<UserConferenceRole>> GetByConferenceIdAsync(int conferenceId);
        Task AddAsync(UserConferenceRole role);
        Task UpdateAsync(UserConferenceRole role);
        Task DeleteAsync(int id);
        Task<bool> IsReviewer(int userId);

    }
}
>>>>>>> origin/Notification
