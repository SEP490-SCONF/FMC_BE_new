using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;


namespace DataAccess
{
    public class UserDAO
    {
        private readonly ConferenceFTestContext _context;

        public UserDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all users
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            try
            {
                return await _context.Users.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all users.", ex);
            }
        }

        // Get user by ID
        public async Task<User> GetUserById(int id)
        {
            try
            {
                return await _context.Users
                                     .Include(u => u.Role) 
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.UserId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving user with ID {id}.", ex);
            }
        }


        // Add a new user
        public async Task AddUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding new user.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding new user.", ex);
            }
        }

        // Update existing user
        public async Task UpdateUser(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.UserId);
                if (existingUser == null)
                    throw new Exception($"User with ID {user.UserId} not found.");

                _context.Entry(existingUser).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating user", ex);
            }
        }


        // Delete user by ID
        public async Task DeleteUser(int id)
        {
            try
            {
                var user = await GetUserById(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"User with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting user.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting user with ID {id}.", ex);
            }
        }

        // Get user by email
        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving user with email: {email}.", ex);
            }
        }

        // Count users
        public async Task<int> GetUserCount()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while counting users.", ex);
            }
        }
        public async Task<User?> GetByRefreshToken(string refreshToken)
        {
            // Tìm user theo RefreshToken, có thể thêm AsNoTracking() nếu chỉ đọc
            return await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }
        public async Task<IEnumerable<User>> GetOrganizers()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.RoleId == 1)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving organizers.", ex);
            }
        }
    }
}
