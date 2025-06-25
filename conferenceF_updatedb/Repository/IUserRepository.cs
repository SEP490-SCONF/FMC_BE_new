using BussinessObject.Entity;


namespace Repository
{
    public interface IUserRepository : IRepositoryBase<User>
    {
       
        Task<User> GetByEmail(string email);
        Task<int> GetUserCount();
        Task<IEnumerable<User>> GetOrganizers();


    }
}
