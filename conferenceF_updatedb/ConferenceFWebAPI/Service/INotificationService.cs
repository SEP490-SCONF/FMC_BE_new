using BussinessObject.Entity;

namespace ConferenceFWebAPI.Service
{
    public interface INotificationService
    {
        Task<bool> SendNotificationToUserAsync(int userId, string title, string content);
        Task<bool> SendNotificationToRoleAsync(int conferenceId, string roleName, string title, string content);
        Task<IEnumerable<Notification>> GetNotificationsForUserAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
    }
}
