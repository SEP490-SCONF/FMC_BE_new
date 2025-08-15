using BussinessObject.Entity;
using DataAccess;


namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDao;

        public UserRepository(UserDAO userDao)
        {
            _userDao = userDao;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userDao.GetAllUsers();
        }

        public async Task<User> GetById(int id)
        {
            return await _userDao.GetUserById(id);
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _userDao.GetUserByEmail(email);
        }

        public async Task Add(User entity)
        {
            await _userDao.AddUser(entity);
        }

        public async Task Update(User entity)
        {
            await _userDao.UpdateUser(entity);
        }

        public async Task Delete(int id)
        {
            await _userDao.DeleteUser(id);
        }

       

        public async Task<int> GetUserCount()
        {
            return await _userDao.GetUserCount();
        }
        public async Task<IEnumerable<User>> GetOrganizers()
        {
            return await _userDao.GetOrganizers();
        }
        public async Task<User?> GetByRefreshToken(string refreshToken)
        {
            return await _userDao.GetByRefreshToken(refreshToken);
        }
        public async Task<bool> RoleExists(int roleId)
        {
            return await _userDao.RoleExists(roleId);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleId(int roleId)
        {
            return await _userDao.GetUsersByRoleId(roleId);
        }


    }
}
