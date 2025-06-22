using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPaymentRepository : IRepositoryBase<Payment>
    {
        Task<IEnumerable<Payment>> GetByUserId(int userId);
    }
}
