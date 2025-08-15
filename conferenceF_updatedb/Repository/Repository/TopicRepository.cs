using BussinessObject.Entity;
using DataAccess;

namespace Repository
{
    public class TopicRepository : ITopicRepository
    {
        private readonly TopicDAO _topicDao;

        public TopicRepository(TopicDAO topicDao)
        {
            _topicDao = topicDao;
        }

        public async Task<IEnumerable<Topic>> GetAll()
        {
            return await _topicDao.GetAllTopics();
        }

        public async Task<Topic> GetById(int id)
        {
            return await _topicDao.GetTopicById(id);
        }

        public async Task Add(Topic entity)
        {
            await _topicDao.AddTopic(entity);
        }

        public async Task Update(Topic entity)
        {
            await _topicDao.UpdateTopic(entity);
        }

        public async Task Delete(int id)
        {
            await _topicDao.DeleteTopic(id);
        }
        public async Task<IEnumerable<Topic>> GetTopicsByConferenceIdAsync(int conferenceId)
        {
            return await _topicDao.GetTopicsByConferenceId(conferenceId);
        }

        //public async Task<IEnumerable<Topic>> GetTopicsByConferenceId(int conferenceId)
        //{
        //    return await _topicDao.GetTopicsByConferenceId(conferenceId);
        //}

        public async Task<IEnumerable<Topic>> GetTopicsByIdsAsync(List<int> topicIds)
        {
            return await _topicDao.GetTopicsByIdsAsync(topicIds);
        }

    }
}
