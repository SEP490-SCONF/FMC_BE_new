using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;


namespace DataAccess
{
    public class RoleDAO
    {
        private readonly ConferenceFTestContext _context;

        public RoleDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all Roles
        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            try
            {
                return await _context.Roles.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all Roles.", ex);
            }
        }

        // Get Role by ID
        public async Task<Role> GetRoleById(int id)
        {
            try
            {
                return await _context.Roles
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.RoleId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving Role with ID {id}.", ex);
            }
        }

        // Add a new Role
        public async Task AddRole(Role Role)
        {
            try
            {
                _context.Roles.Add(Role);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding new Role.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding new Role.", ex);
            }
        }

        // Update existing Role
        public async Task UpdateRole(Role Role)
        {
            try
            {
                var existingRole = await _context.Roles.FindAsync(Role.RoleId);
                if (existingRole == null)
                    throw new Exception($"Role with ID {Role.RoleId} not found.");

                _context.Entry(existingRole).CurrentValues.SetValues(Role);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating Role", ex);
            }
        }


        // Delete Role by ID
        public async Task DeleteRole(int id)
        {
            try
            {
                var Role = await GetRoleById(id);
                if (Role != null)
                {
                    _context.Roles.Remove(Role);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Role with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting Role.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting Role with ID {id}.", ex);
            }
        }

        

        // Count Roles
        public async Task<int> GetRoleCount()
        {
            try
            {
                return await _context.Roles.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while counting Roles.", ex);
            }
        }
    }
}
