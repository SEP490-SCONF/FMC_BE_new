using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class HighlightAreaDAO
    {
        private readonly ConferenceFTestContext _context;

        public HighlightAreaDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HighlightArea>> GetAllAsync()
        {
            return await _context.HighlightAreas.AsNoTracking().ToListAsync();
        }

        public async Task<HighlightArea> GetByIdAsync(int id)
        {
            return await _context.HighlightAreas.FindAsync(id);
        }

        public async Task<IEnumerable<HighlightArea>> GetByHighlightIdAsync(int highlightId)
        {
            return await _context.HighlightAreas
                .Where(h => h.HighlightId == highlightId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(HighlightArea entity)
        {
            await _context.HighlightAreas.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HighlightArea entity)
        {
            _context.HighlightAreas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.HighlightAreas.FindAsync(id);
            if (entity != null)
            {
                _context.HighlightAreas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
