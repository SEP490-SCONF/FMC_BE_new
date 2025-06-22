using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface INotificationRepository : IRepositoryBase<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserId(int userId);
    }
}
