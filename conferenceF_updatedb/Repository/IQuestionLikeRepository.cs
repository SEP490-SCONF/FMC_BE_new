using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IQuestionLikeRepository : IRepositoryBase<QuestionLike>
    {
        Task<IEnumerable<QuestionLike>> GetByQuestionId(int questionId);
    }
}
