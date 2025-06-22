using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IAnswerQuestionRepository : IRepositoryBase<AnswerQuestion>
    {
        Task<IEnumerable<AnswerQuestion>> GetByQuestionId(int questionId);
    }
}
