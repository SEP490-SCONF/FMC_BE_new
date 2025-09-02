using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaymentRepository : IRepositoryBase<Payment>
    {
        Task<IEnumerable<Payment>> GetAll();
        Task<Payment> GetById(int id);
        Task<IEnumerable<Payment>> GetByUserId(int userId);
        Task<IEnumerable<Payment>> GetByConferenceId(int conferenceId);
        Task<IEnumerable<Payment>> GetByStatus(string status);
        Task<IEnumerable<Payment>> GetRecentPayments(DateTime fromDate);
        Task Add(Payment payment);
        Task<Payment?> GetByOrderCode(String orderCode);
        Task<Payment?> GetLatestPendingByUserId(int userId);
        Task Update(Payment payment);
        Task Delete(int id);
        Task<bool> HasUserPaidFee(int userId, int conferenceId, int feeDetailId);
        Task<FeeDetail?> GetFeeDetailByIdAsync(int feeDetailId);
        Task<List<FeeDetail>> GetFeeDetailsByIdsAsync(List<int> feeDetailIds);


    }
}
