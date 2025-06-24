using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationForUserAsync(int userId, string title, string content);
        Task<(Notification?, List<User>)> CreateNotificationForRoleAsync(int conferenceId, string roleName, string title, string content);
    }
}
