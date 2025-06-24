using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PaperDAO
    {
        private readonly ConferenceFTestContext _context;

        public PaperDAO(ConferenceFTestContext context)
        {
            _context = context;
        }
        public IQueryable<Paper> GetAllPapers()
        {
            return _context.Papers.Where(p => p.Status != "Deleted").AsQueryable();
        }
        public async Task<Paper> GetByIdAsync(int id)
        {
            return await _context.Papers.FindAsync(id);
        }

        public async Task AddAsync(Paper entity)
        {
            await _context.Papers.AddAsync(entity);
        }

        public void Update(Paper entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(Paper entity)
        {
            _context.Papers.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

