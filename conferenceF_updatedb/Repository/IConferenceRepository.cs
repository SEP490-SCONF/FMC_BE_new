using BussinessObject.Entity;


namespace Repository
{
    public interface IConferenceRepository : IRepositoryBase<Conference>
    {
        Task<int> GetConferenceCount();
        Task UpdateConferenceStatus(int conferenceId, bool newStatus);
        Task<IEnumerable<Conference>> GetAllConferencesFalse();
        IQueryable<Conference> GetAllQueryable();
        Task<Conference> Insert(Conference conference);
    }
}
