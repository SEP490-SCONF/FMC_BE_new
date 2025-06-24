using BussinessObject.Entity;
using DataAccess;


namespace Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleDAO _RoleDao;

        public RoleRepository(RoleDAO RoleDao)
        {
            _RoleDao = RoleDao;
        }

        public async Task<IEnumerable<Role>> GetAll()
        {
            return await _RoleDao.GetAllRoles();
        }

        public async Task<Role> GetById(int id)
        {
            return await _RoleDao.GetRoleById(id);
        }

        public async Task Add(Role entity)
        {
            await _RoleDao.AddRole(entity);
        }

        public async Task Update(Role entity)
        {
            await _RoleDao.UpdateRole(entity);
        }

        public async Task Delete(int id)
        {
            await _RoleDao.DeleteRole(id);
        }



        public async Task<int> GetRoleCount()
        {
            return await _RoleDao.GetRoleCount();
        }
    }
}
