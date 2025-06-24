using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataAccess
{
    public class NotificationDAO
    {
        private readonly ConferenceFTestContext _context;

        public NotificationDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<User>> GetUsersInRoleAsync(int conferenceId, string roleName)
        {
            return await _context.UserConferenceRoles
                .Where(ucr => ucr.ConferenceId == conferenceId && ucr.ConferenceRole.RoleName == roleName)
                .Select(ucr => ucr.User)
                .ToListAsync();
        }


    }
}
