using BussinessObject.Entity;


namespace Repository
{
    public interface IUserRepository : IRepositoryBase<User>
    {
       
        Task<User> GetByEmail(string email);
        Task<int> GetUserCount();
        Task<IEnumerable<User>> GetOrganizers();
        Task<User?> GetByRefreshToken(string refreshToken);
        Task<bool> RoleExists(int roleId);
        Task<IEnumerable<User>> GetUsersByRoleId(int roleId);


    }
}
