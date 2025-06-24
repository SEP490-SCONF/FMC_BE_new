using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDAO;

        public NotificationRepository(NotificationDAO notificationDAO)
        {
            _notificationDAO = notificationDAO;
        }

        public async Task<Notification> CreateNotificationForUserAsync(int userId, string title, string content)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            // Thêm trạng thái "chưa đọc" cho người dùng
            notification.NotificationStatuses.Add(new NotificationStatus
            {
                UserId = userId,
                IsRead = false
            });

            return await _notificationDAO.AddNotificationAsync(notification);
        }

        public async Task<(Notification?, List<User>)> CreateNotificationForRoleAsync(int conferenceId, string roleName, string title, string content)
        {
            // 1. Tìm tất cả người dùng có vai trò đó
            var usersInRole = await _notificationDAO.GetUsersInRoleAsync(conferenceId, roleName);
            if (!usersInRole.Any())
            {
                return (null, new List<User>());
            }

            // 2. Tạo thông báo chung
            var notification = new Notification
            {
                RoleTarget = roleName,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Thêm trạng thái "chưa đọc" cho từng người dùng
            foreach (var user in usersInRole)
            {
                notification.NotificationStatuses.Add(new NotificationStatus
                {
                    UserId = user.UserId,
                    IsRead = false
                });
            }

            var createdNotification = await _notificationDAO.AddNotificationAsync(notification);
            return (createdNotification, usersInRole);
        }
    }
}
