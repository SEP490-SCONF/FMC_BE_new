using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace DataAccess
    {
        public class CallForPaperDAO
        {
            private readonly ConferenceFTestContext _dbContext;

            public CallForPaperDAO(ConferenceFTestContext dbContext)
            {
                _dbContext = dbContext;
            }

        public async Task<IEnumerable<CallForPaper>> GetAllCallForPapers()
        {
            return await _dbContext.CallForPapers
                .Include(cf => cf.Conference)
                    .ThenInclude(c => c.Topics)
                .ToListAsync();
        }


        public async Task<IEnumerable<CallForPaper>> GetCallForPapersByConferenceId(int conferenceId)
        {
            return await _dbContext.CallForPapers
                .Where(cf => cf.ConferenceId == conferenceId)
                .Include(cf => cf.Conference)
                    .ThenInclude(c => c.Topics)
                .ToListAsync();
        }
        public async Task<CallForPaper?> GetCallForPaperById(int id)
        {
            return await _dbContext.CallForPapers
                .Include(cf => cf.Conference)
                    .ThenInclude(c => c.Topics)
                .FirstOrDefaultAsync(cf => cf.Cfpid == id);
        }


        /// <summary>
        /// Thêm một CallForPaper mới.
        /// </summary>
        public async Task AddCallForPaper(CallForPaper callForPaper)
            {
                if (callForPaper == null)
                {
                    throw new ArgumentNullException(nameof(callForPaper));
                }
                callForPaper.CreatedAt = DateTime.UtcNow; // Tự động set thời gian tạo
                await _dbContext.CallForPapers.AddAsync(callForPaper);
                await _dbContext.SaveChangesAsync();
            }

            /// <summary>
            /// Cập nhật một CallForPaper hiện có.
            /// </summary>
            public async Task UpdateCallForPaper(CallForPaper callForPaper)
            {
                if (callForPaper == null)
                {
                    throw new ArgumentNullException(nameof(callForPaper));
                }

                _dbContext.CallForPapers.Update(callForPaper);
                await _dbContext.SaveChangesAsync();
            }

            public async Task DeleteCallForPaper(int id)
            {
                var callForPaper = await _dbContext.CallForPapers.FirstOrDefaultAsync(cf => cf.Cfpid == id);
                if (callForPaper != null)
                {
                    _dbContext.CallForPapers.Remove(callForPaper);
                    await _dbContext.SaveChangesAsync();
                }
            }

        public async Task<bool> HasActiveCallForPaper(int conferenceId)
        {
            return await _dbContext.CallForPapers
                .AnyAsync(cfp => cfp.ConferenceId == conferenceId && cfp.Status == true);
        }
    }
}
