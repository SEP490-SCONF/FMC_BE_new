using BussinessObject.Entity;


namespace Repository
{
    public interface ITopicRepository : IRepositoryBase<Topic>
    {
        //Task<IEnumerable<Topic>> GetTopicsByConferenceId(int conferenceId);
        Task<IEnumerable<Topic>> GetTopicsByConferenceIdAsync(int conferenceId);
        Task<IEnumerable<Topic>> GetTopicsByIdsAsync(List<int> topicIds);


    }
}
