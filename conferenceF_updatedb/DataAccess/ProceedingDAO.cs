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

        public async Task<Proceeding> CreateProceedingAsync(Proceeding proceeding)
        {
            _context.Proceedings.Add(proceeding);
            await _context.SaveChangesAsync();
            return proceeding;
        }

        public async Task<Proceeding?> GetProceedingByIdAsync(int proceedingId)
        {
            return await _context.Proceedings
                                 .Include(p => p.Papers)
                                 .FirstOrDefaultAsync(p => p.ProceedingId == proceedingId);
        }

        public async Task<Proceeding?> GetProceedingByConferenceIdAsync(int conferenceId)
        {
            return await _context.Proceedings
                                 .Include(p => p.Papers)
                                 .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId);
        }

        public async Task UpdateProceedingAsync(Proceeding proceeding)
        {
            _context.Proceedings.Update(proceeding);
            await _context.SaveChangesAsync();
        }

        // Lấy danh sách các bài báo đã được chấp nhận và xuất bản
        public async Task<List<Paper>> GetPublishedPapersByConferenceAsync(int conferenceId)
        {
            return await _context.Papers
                                 .Where(p => p.ConferenceId == conferenceId && p.Status == "Accepted" && p.IsPublished == true)
                                 .ToListAsync();
        }

    }

}
