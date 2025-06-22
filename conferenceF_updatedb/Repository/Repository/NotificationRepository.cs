using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDao;

        public NotificationRepository(NotificationDAO notificationDao)
        {
            _notificationDao = notificationDao;
        }

        public async Task<IEnumerable<Notification>> GetAll()
        {
            return await _notificationDao.GetAll();
        }

        public async Task<Notification> GetById(int id)
        {
            return await _notificationDao.GetById(id);
        }

        public async Task Add(Notification entity)
        {
            await _notificationDao.Add(entity);
        }

        public async Task Update(Notification entity)
        {
            await _notificationDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _notificationDao.Delete(id);
        }

        public async Task<IEnumerable<Notification>> GetByUserId(int userId)
        {
            return await _notificationDao.GetByUserId(userId);
        }
    }
}
