using BussinessObject.Entity;
using DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ForumRepository : IForumRepository
    {
        private readonly ForumDAO _forumDao;

        public ForumRepository(ForumDAO forumDao)
        {
            _forumDao = forumDao;
        }

        public async Task<IEnumerable<Forum>> GetAll()
        {
            return await _forumDao.GetAll();
        }

        public async Task<Forum> GetById(int id)
        {
            return await _forumDao.GetById(id);
        }

        public async Task Add(Forum entity)
        {
            await _forumDao.Add(entity);
        }

        public async Task Update(Forum entity)
        {
            await _forumDao.Update(entity);
        }

        public async Task Delete(int id)
        {
            await _forumDao.Delete(id);
        }

        public async Task<Forum> GetByConferenceId(int conferenceId)
        {
            return await _forumDao.GetByConferenceId(conferenceId);
        }
    }
}
