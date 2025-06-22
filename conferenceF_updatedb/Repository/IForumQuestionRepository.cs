using BussinessObject.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IForumQuestionRepository : IRepositoryBase<ForumQuestion>
    {
        Task<IEnumerable<ForumQuestion>> GetByForumId(int forumId);
    }
}
