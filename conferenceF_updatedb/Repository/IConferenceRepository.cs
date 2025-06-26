using BussinessObject.Entity;


namespace Repository
{
    public interface IConferenceRepository : IRepositoryBase<Conference>
    {
        Task<int> GetConferenceCount();
        Task UpdateConferenceStatus(int conferenceId, bool newStatus);
    }
}
