using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IAnswerLikeRepository : IRepositoryBase<AnswerLike>
    {
        Task<IEnumerable<AnswerLike>> GetByAnswerId(int answerId);
    }
}
