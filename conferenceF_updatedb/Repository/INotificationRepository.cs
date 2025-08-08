using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
    }
}
