using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ForumQuestionRepository : IForumQuestionRepository
    {
        private readonly ForumQuestionDAO _dao;

        public ForumQuestionRepository(ForumQuestionDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<ForumQuestion>> GetAll()
        {
            return await _dao.GetAll();
        }

        public async Task<ForumQuestion> GetById(int id)
        {
            return await _dao.GetById(id);
        }

        public async Task Add(ForumQuestion entity)
        {
            await _dao.Add(entity);
        }

        public async Task Update(ForumQuestion entity)
        {
            await _dao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _dao.Delete(id);
        }

        public async Task<IEnumerable<ForumQuestion>> GetByForumId(int forumId)
        {
            return await _dao.GetByForumId(forumId);
        }
    }
}
