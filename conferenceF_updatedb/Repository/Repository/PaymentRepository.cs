using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Payment>> GetByStatus(string status)
        {
            return await _paymentDao.GetByStatus(status);
        }
        public async Task<IEnumerable<Payment>> GetRecentPayments(DateTime fromDate)
        {
            return await _paymentDao.GetRecentPayments(fromDate);
        }

        public async Task<IEnumerable<Payment>> GetByConferenceId(int conferenceId)
        {
            return await _paymentDao.GetByConferenceId(conferenceId);
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
        public async Task<Payment?> GetByOrderCode(String orderCode)
        {
            return await _paymentDao.GetByOrderCode(orderCode);
        }

        public async Task Update(Payment entity)
        {
            await _paymentDao.Update(entity);
        }
        public async Task<Payment?> GetLatestPendingByUserId(int userId)
        {
            return await _paymentDao.GetLatestPendingByUserId(userId);
        }
        public async Task Delete(int id)
        {
            await _paymentDao.Delete(id);
        }

        public async Task<IEnumerable<Payment>> GetByUserId(int userId)
        {
            return await _paymentDao.GetByUserId(userId);
        }
        public async Task<bool> HasUserPaidFee(int userId, int conferenceId, int feeDetailId)
        {
            return await _paymentDao.HasUserPaidFee(userId, conferenceId, feeDetailId);
        }

        public async Task<FeeDetail?> GetFeeDetailByIdAsync(int feeDetailId)
        {
            return await _paymentDao.GetFeeDetailByIdAsync(feeDetailId);
        }



    }
}
