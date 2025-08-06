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
    public class UserConferenceRoleDAO
    {
        private readonly ConferenceFTestContext _context;

        public UserConferenceRoleDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserConferenceRole>> GetUsersByConferenceIdAndRoles(int conferenceId, List<int> roleIds)
        {
            return await _context.UserConferenceRoles
        .Where(ucr => ucr.ConferenceId == conferenceId && roleIds.Contains(ucr.ConferenceRoleId))
        .Include(ucr => ucr.User)
        .Include(ucr => ucr.Conference)
        .Include(ucr => ucr.ConferenceRole)
        .AsNoTracking()
        .ToListAsync();
        }
        public async Task<IEnumerable<UserConferenceRole>> GetAll()
        {
            try
            {
                return await _context.UserConferenceRoles
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user conference roles.", ex);
            }
        }

        public async Task<UserConferenceRole> GetById(int id)
        {
            try
            {
                return await _context.UserConferenceRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving role with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<UserConferenceRole>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.UserConferenceRoles
                 .Where(u => u.ConferenceId == conferenceId)
                .Include(u => u.User)
                 .Include(u => u.Conference)
                     .Include(u => u.ConferenceRole)
                    .AsNoTracking()
                    .ToListAsync();
                }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving roles for conference ID {conferenceId}.", ex);
            }
        }

        


        public async Task Add(UserConferenceRole entity)
        {
            try
            {
                _context.UserConferenceRoles.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding user conference role.", ex);
            }
        }

        public async Task Update(UserConferenceRole entity)
        {
            try
            {
                var existing = await _context.UserConferenceRoles.FindAsync(entity.Id);
                if (existing == null)
                    throw new Exception($"UserConferenceRole with ID {entity.Id} not found.");

                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user conference role with ID {entity.Id}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var entity = await _context.UserConferenceRoles.FindAsync(id);
                if (entity != null)
                {
                    _context.UserConferenceRoles.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"UserConferenceRole with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user conference role with ID {id}.", ex);
            }
        }

        public async Task<bool> IsReviewer(int userId)
        {
            return await _context.UserConferenceRoles
                .AnyAsync(ucr => ucr.UserId == userId && ucr.ConferenceRoleId == 3);
        }

        public async Task<IEnumerable<UserConferenceRole>> GetByCondition(Expression<Func<UserConferenceRole, bool>> predicate)
        {
            return await _context.UserConferenceRoles.Where(predicate).ToListAsync();
        }
        public async Task<UserConferenceRole?> UpdateConferenceRoleForUser(int userId, int conferenceId, int newConferenceRoleId)
        {
            var existingAssignment = await _context.UserConferenceRoles
                                                    .FirstOrDefaultAsync(ucr =>
                                                        ucr.UserId == userId &&
                                                        ucr.ConferenceId == conferenceId);

            if (existingAssignment == null)
            {
                return null; // Không tìm thấy bản ghi để cập nhật
            }

            existingAssignment.ConferenceRoleId = newConferenceRoleId;

            await _context.SaveChangesAsync();
            return existingAssignment;
        }

        public async Task<List<Conference>> GetConferencesByUserIdAndRoleAsync(int userId, string roleName)
        {
            // Tìm ConferenceRoleId cho vai trò "Organizer"
            var role = await _context.ConferenceRoles
                                      .FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (role == null)
                return new List<Conference>(); // Trả về danh sách rỗng nếu không tìm thấy vai trò

            // Truy vấn UserConferenceRoles để tìm hội thảo mà người dùng có vai trò cụ thể
            return await _context.UserConferenceRoles
                                 .Where(ucr => ucr.UserId == userId && ucr.ConferenceRoleId == role.ConferenceRoleId)
                                 .Select(ucr => ucr.Conference)
                                 .Distinct() // Đảm bảo không có hội thảo trùng lặp
                                 .ToListAsync();
        }



        public async Task<UserConferenceRole?> GetReviewerByUserAndConference(int userId, int conferenceId)
        {
            return await _context.UserConferenceRoles
                .Include(u => u.User)
                .Include(u => u.Conference)
                .Include(u => u.ConferenceRole)
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.ConferenceId == conferenceId &&
                    u.ConferenceRoleId == 3); // 3 là Reviewer
        }


    }

}
