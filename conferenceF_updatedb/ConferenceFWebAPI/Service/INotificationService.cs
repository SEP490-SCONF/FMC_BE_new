using BussinessObject.Entity;

namespace ConferenceFWebAPI.Service
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsForUserAsync(int userId);

    }
}
