using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataAccess
{
    public class ForumDAO
    {
        private readonly ConferenceFTestContext _context;

        public ForumDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Forum>> GetAll()
        {
            try
            {
                return await _context.Forums
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all forums.", ex);
            }
        }

        public async Task<Forum> GetById(int id)
        {
            try
            {
                return await _context.Forums
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(f => f.ForumId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving forum with ID {id}.", ex);
            }
        }

        public async Task<Forum> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.Forums
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(f => f.ConferenceId == conferenceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving forum for conference ID {conferenceId}.", ex);
            }
        }

        public async Task Add(Forum forum)
        {
            try
            {
                _context.Forums.Add(forum);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding forum.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding forum.", ex);
            }
        }

        public async Task Update(Forum forum)
        {
            try
            {
                var existing = await _context.Forums.FindAsync(forum.ForumId);
                if (existing == null)
                    throw new Exception($"Forum with ID {forum.ForumId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(forum);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating forum with ID {forum.ForumId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var forum = await _context.Forums.FindAsync(id);
                if (forum != null)
                {
                    _context.Forums.Remove(forum);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Forum with ID {id} not found for deletion.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting forum with ID {id}.", ex);
            }
        }
    }
}
