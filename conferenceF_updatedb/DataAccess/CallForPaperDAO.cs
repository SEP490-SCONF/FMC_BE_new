using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataAccess
{
    public class CallForPaperDAO
    {
        private readonly ConferenceFTestContext _context;

        public CallForPaperDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CallForPaper>> GetAll()
        {
            try
            {
                return await _context.CallForPapers
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving all call for papers.", ex);
            }
        }

        public async Task<CallForPaper> GetById(int id)
        {
            try
            {
                return await _context.CallForPapers
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.Cfpid == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving call for paper with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<CallForPaper>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.CallForPapers
                                     .Where(c => c.ConferenceId == conferenceId)
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving call for papers for conference ID {conferenceId}.", ex);
            }
        }

        public async Task Add(CallForPaper entity)
        {
            try
            {
                _context.CallForPapers.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding call for paper.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding call for paper.", ex);
            }
        }

        public async Task Update(CallForPaper entity)
        {
            try
            {
                var existing = await _context.CallForPapers.FindAsync(entity.Cfpid);
                if (existing == null)
                    throw new Exception($"Call for paper with ID {entity.Cfpid} not found.");

                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating call for paper with ID {entity.Cfpid}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var entity = await _context.CallForPapers.FindAsync(id);
                if (entity != null)
                {
                    _context.CallForPapers.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Call for paper with ID {id} not found for deletion.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting call for paper with ID {id}.", ex);
            }
        }
    }
}
