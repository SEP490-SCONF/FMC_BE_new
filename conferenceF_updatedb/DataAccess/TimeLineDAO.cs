using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TimeLineDAO
    {
        private readonly ConferenceFTestContext _context;

        public TimeLineDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        public async Task<TimeLine?> GetByIdAsync(int id)
        {
            return await _context.TimeLines.FindAsync(id);
        }

        public async Task<List<TimeLine>> GetByConferenceIdAsync(int conferenceId)
        {
            return await _context.TimeLines
                .Where(t => t.ConferenceId == conferenceId)
                .OrderBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<TimeLine> AddAsync(TimeLine timeLine)
        {
            await _context.TimeLines.AddAsync(timeLine);
            await _context.SaveChangesAsync();
            return timeLine;
        }

        public async Task UpdateAsync(TimeLine timeLine)
        {
            _context.Entry(timeLine).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var timeLine = await _context.TimeLines.FindAsync(id);
            if (timeLine == null)
                return false;

            _context.TimeLines.Remove(timeLine);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
