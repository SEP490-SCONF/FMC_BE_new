using ConferenceFWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepo;

        // Service bây giờ phụ thuộc vào Repository thay vì DbContext
        public NotificationService(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepo)
        {
            _hubContext = hubContext;
            _notificationRepo = notificationRepo;
        }

        public async Task SendNotificationToUserAsync(int userId, string title, string content)
        {
            // 1. Logic tạo và lưu thông báo được chuyển xuống Repository
            var notification = await _notificationRepo.CreateNotificationForUserAsync(userId, title, content);

            // 2. Service chỉ còn nhiệm vụ gửi thông báo real-time
            if (notification != null)
            {
                await _hubContext.Clients.User(userId.ToString())
                                 .SendAsync("ReceiveNotification", notification);
            }
        }

        public async Task SendNotificationToRoleAsync(int conferenceId, string roleName, string title, string content)
        {
            // 1. Lấy thông báo và danh sách người dùng từ Repository
            var (notification, usersInRole) = await _notificationRepo.CreateNotificationForRoleAsync(conferenceId, roleName, title, content);

            // 2. Gửi thông báo nếu có người dùng và thông báo được tạo thành công
            if (notification != null && usersInRole.Any())
            {
                var userIds = usersInRole.Select(u => u.UserId.ToString()).ToList();
                await _hubContext.Clients.Users(userIds)
                                 .SendAsync("ReceiveNotification", notification);
            }
        }
    }
}
