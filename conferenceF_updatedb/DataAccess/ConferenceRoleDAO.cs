using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ConferenceRoleDAO
    {
        private readonly ConferenceFTestContext _context;

        public ConferenceRoleDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all ConferenceRoles
        public async Task<IEnumerable<ConferenceRole>> GetAllConferenceRoles()
        {
            try
            {
                return await _context.ConferenceRoles
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all ConferenceRoles.", ex);
            }
        }

        // Get ConferenceRole by ID
        public async Task<ConferenceRole> GetConferenceRoleById(int id)
        {
            try
            {
                return await _context.ConferenceRoles
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(i => i.ConferenceRoleId ==  id);


            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving ConferenceRole with ID {id}.", ex);
            }
        }

        // Add a new ConferenceRole
        public async Task AddConferenceRole(ConferenceRole ConferenceRole)
        {
            try
            {
                _context.ConferenceRoles.Add(ConferenceRole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding a new ConferenceRole.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding a new ConferenceRole.", ex);
            }
        }

        // Update existing ConferenceRole
        public async Task UpdateConferenceRole(ConferenceRole ConferenceRole)
        {
            try
            {
                var existing = await GetConferenceRoleById(ConferenceRole.ConferenceRoleId);
                if (existing == null)
                    throw new Exception($"ConferenceRole with ID {ConferenceRole.ConferenceRoleId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(ConferenceRole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the ConferenceRole.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating ConferenceRole with ID {ConferenceRole.ConferenceRoleId}.", ex);
            }
        }

        // Delete ConferenceRole by ID
        public async Task DeleteConferenceRole(int id)
        {
            try
            {
                var ConferenceRole = await GetConferenceRoleById(id);
                if (ConferenceRole != null)
                {
                    _context.ConferenceRoles.Remove(ConferenceRole);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"ConferenceRole with ID {id} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting the ConferenceRole.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting ConferenceRole with ID {id}.", ex);
            }
        }

        // Count ConferenceRoles
        public async Task<int> GetConferenceRoleCount()
        {
            try
            {
                return await _context.ConferenceRoles.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while counting ConferenceRoles.", ex);
            }
        }

        public async Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate)
        {
            return await _context.UserConferenceRoles.Where(predicate).ToListAsync();
        }
    }
}
