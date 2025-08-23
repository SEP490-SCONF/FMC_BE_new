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
        public async Task<string?> GetFilePathByConferenceIdAsync(int conferenceId)
        {
            return await _context.Proceedings
                                 .Where(p => p.ConferenceId == conferenceId)
                                 .Select(p => p.FilePath)
                                 .FirstOrDefaultAsync();
        }
        public async Task<Proceeding> UpdateAsync(Proceeding proceeding)
        {
            _context.Proceedings.Update(proceeding);
            await _context.SaveChangesAsync();

            return await _context.Proceedings
                .Include(p => p.PublishedByNavigation)
                .Include(p => p.Papers)
                .FirstOrDefaultAsync(p => p.ProceedingId == proceeding.ProceedingId);
        }


        public async Task<Proceeding?> GetByIdAsync(int id)
        {
            return await _context.Proceedings
                                 .Include(p => p.Papers)
                                 .FirstOrDefaultAsync(p => p.ProceedingId == id);
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
                                 .Include(p=> p.PublishedByNavigation)
                                 .Include(p => p.Conference) 
                                 .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId);
        }



        // Lấy danh sách các bài báo đã được chấp nhận và xuất bản
        public async Task<List<Paper>> GetPublishedPapersByConferenceAsync(int conferenceId)
        {
            return await _context.Papers
                                 .Where(p => p.ConferenceId == conferenceId && p.Status == "Accepted" && p.IsPublished == true)
                                 .ToListAsync();
        }

        public async Task<List<Proceeding>> GetAllProceedingsAsync()
        {
            return await _context.Proceedings
                                 .Include(p => p.Papers)
                                 .Include(p => p.PublishedByNavigation)
                                 .Include(p => p.Conference)
                                 .ToListAsync();
        }

    }

}
