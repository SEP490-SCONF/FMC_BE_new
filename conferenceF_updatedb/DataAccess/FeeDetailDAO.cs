using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FeeDetailDAO
    {
        private readonly ConferenceFTestContext _context;
        public FeeDetailDAO(ConferenceFTestContext context) { _context = context; }

        public IQueryable<FeeDetail> GetAllQueryable() => _context.FeeDetails.Include(f => f.FeeType).Include(f => f.Conference).AsQueryable();

        public async Task<IEnumerable<FeeDetail>> GetAll() => await _context.FeeDetails.Include(f => f.FeeType).Include(f => f.Conference).ToListAsync();

        public async Task<FeeDetail> GetById(int id) => await _context.FeeDetails.Include(f => f.FeeType).Include(f => f.Conference).FirstOrDefaultAsync(f => f.FeeDetailId == id);

        public async Task Add(FeeDetail entity)
        {
            _context.FeeDetails.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(FeeDetail entity)
        {
            _context.FeeDetails.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _context.FeeDetails.FindAsync(id);
            if (entity != null)
            {
                _context.FeeDetails.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FeeDetail>> GetByConferenceId(int conferenceId)
        {
            return await _context.FeeDetails
                .Where(f => f.ConferenceId == conferenceId)
                .Include(f => f.FeeType)
                .ToListAsync();
        }
    }
}