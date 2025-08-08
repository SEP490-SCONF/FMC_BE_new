using BussinessObject.Entity;
using ConferenceFWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<Notification>> GetNotificationsForUserAsync(int userId)
        {
            // Có thể thêm logic nghiệp vụ ở đây, ví dụ: phân trang, lọc
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }
    }
}
