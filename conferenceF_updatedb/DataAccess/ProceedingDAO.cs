using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ProceedingDAO
    {
        private readonly ConferenceFTestContext _context;

        public ProceedingDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Proceeding>> GetAll()
        {
            try
            {
                return await _context.Proceedings.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all proceedings.", ex);
            }
        }

        public async Task<Proceeding> GetById(int id)
        {
            try
            {
                return await _context.Proceedings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProceedingId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving proceeding with ID {id}.", ex);
            }
        }

        public async Task<IEnumerable<Proceeding>> GetByConferenceId(int conferenceId)
        {
            try
            {
                return await _context.Proceedings
                    .Where(p => p.ConferenceId == conferenceId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving proceedings for conference ID {conferenceId}.", ex);
            }
        }

        public async Task Add(Proceeding proceeding)
        {
            try
            {
                _context.Proceedings.Add(proceeding);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding proceeding.", ex);
            }
        }

        public async Task Update(Proceeding proceeding)
        {
            try
            {
                var existing = await _context.Proceedings.FindAsync(proceeding.ProceedingId);
                if (existing == null)
                    throw new Exception($"Proceeding with ID {proceeding.ProceedingId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(proceeding);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating proceeding with ID {proceeding.ProceedingId}.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var proceeding = await _context.Proceedings.FindAsync(id);
                if (proceeding != null)
                {
                    _context.Proceedings.Remove(proceeding);
                    await _context.SaveChangesAsync();  
                }
                else
                {
                    throw new Exception($"Proceeding with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting proceeding with ID {id}.", ex);
            }
        }


    }

}
