using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FeeTypeDAO
    {
        private readonly ConferenceFTestContext _context;
        public FeeTypeDAO(ConferenceFTestContext context) { _context = context; }

        public IQueryable<FeeType> GetAllQueryable() => _context.FeeTypes.Include(f => f.FeeDetails).AsQueryable();

        public async Task<IEnumerable<FeeType>> GetAll() => await _context.FeeTypes.Include(f => f.FeeDetails).ToListAsync();

        public async Task<FeeType> GetById(int id) => await _context.FeeTypes.Include(f => f.FeeDetails).FirstOrDefaultAsync(f => f.FeeTypeId == id);

        public async Task Add(FeeType entity)
        {
            _context.FeeTypes.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(FeeType entity)
        {
            _context.FeeTypes.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _context.FeeTypes.FindAsync(id);
            if (entity != null)
            {
                _context.FeeTypes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}