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

        public async Task AddAsync(Notification entity)
        {
            await _context.Notifications.AddAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Thêm phương thức để lấy tất cả thông báo của một người dùng, sắp xếp theo thời gian mới nhất
        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                                 .Where(n => n.UserId == userId)
                                 .OrderByDescending(n => n.CreatedAt)
                                 .ToListAsync();
        }

    }
}
