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

        public async Task<IEnumerable<Notification>> GetAll()
        {
            try
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all notifications.", ex);
            }
        }

        public async Task<Notification> GetById(int id)
        {
            try
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NotiId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving notification with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Notification>> GetByUserId(int userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving notifications for user ID {userId}.", ex);
            }
        }

        public async Task Add(Notification notification)
        {
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new notification.", ex);
            }
        }

        public async Task Update(Notification notification)
        {
            try
            {
                var existing = await _context.Notifications.FindAsync(notification.NotiId);
                if (existing == null)
                    throw new Exception($"Notification with ID {notification.NotiId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(notification);
                await _context.SaveChangesAsync();
            }   
            catch (Exception ex)
            {
                throw new Exception($"Error updating notification with ID {notification.NotiId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Notification with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting notification with ID {id}.", ex);
            }
        }
    }
}
