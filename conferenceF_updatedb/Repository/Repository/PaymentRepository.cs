using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDAO _paymentDao;

        public PaymentRepository(PaymentDAO paymentDao)
        {
            _paymentDao = paymentDao;
        }

        public async Task<IEnumerable<Payment>> GetAll()
        {
            return await _paymentDao.GetAll();
        }

        public async Task<Payment> GetById(int id)
        {
            return await _paymentDao.GetById(id);
        }

        public async Task Add(Payment entity)
        {
            await _paymentDao.Add(entity);
        }

        public async Task Update(Payment entity)
        {
            await _paymentDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _paymentDao.Delete(id);
        }

        public async Task<IEnumerable<Payment>> GetByUserId(int userId)
        {
            return await _paymentDao.GetByUserId(userId);
        }
    }
}
