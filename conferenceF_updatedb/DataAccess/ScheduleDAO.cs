using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ScheduleDAO
    {
        private readonly ConferenceFTestContext _context;

        public ScheduleDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all schedules (basic)
        public async Task<Schedule?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.Schedules
                .Include(s => s.Paper)         
                .Include(s => s.Conference)     
                .Include(s => s.Presenter)      
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        }

        // Hàm lấy tất cả lịch của một hội thảo
        public async Task<List<Schedule>> GetSchedulesByConferenceIdAsync(int conferenceId)
        {
            return await _context.Schedules
                .Where(s => s.ConferenceId == conferenceId)
                .Include(s => s.Paper)         
                .Include(s => s.Conference)     
                .Include(s => s.Presenter)     
                .OrderBy(s => s.PresentationStartTime)
                .ToListAsync();
        }

        // Hàm cập nhật lịch
        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            // Nếu có PaperId nhưng PresenterId null, tự động lấy author đầu tiên của paper
            if (schedule.PaperId.HasValue && !schedule.PresenterId.HasValue)
            {
                var paper = await _context.Papers
                    .Include(p => p.PaperAuthors)
                    .ThenInclude(pa => pa.Author)
                    .FirstOrDefaultAsync(p => p.PaperId == schedule.PaperId.Value);

                if (paper != null)
                {
                    var firstAuthor = paper.PaperAuthors.FirstOrDefault()?.Author;
                    if (firstAuthor != null)
                    {
                        schedule.PresenterId = firstAuthor.UserId;
                    }
                }
            }

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }


        // Hàm xóa lịch
        public async Task DeleteScheduleAsync(int scheduleId)
        {
            var scheduleToDelete = await _context.Schedules.FindAsync(scheduleId);
            if (scheduleToDelete != null)
            {
                _context.Schedules.Remove(scheduleToDelete);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Schedule> AddScheduleAsync(Schedule schedule)
        {
            // Nếu có PaperId nhưng PresenterId null, tự động lấy author đầu tiên của paper
            if (schedule.PaperId.HasValue && !schedule.PresenterId.HasValue)
            {
                var paper = await _context.Papers
                    .Include(p => p.PaperAuthors)       // PaperAuthors liên kết Author
                    .ThenInclude(pa => pa.Author)
                    .FirstOrDefaultAsync(p => p.PaperId == schedule.PaperId.Value);

                if (paper != null)
                {
                    // Lấy author đầu tiên làm presenter
                    var firstAuthor = paper.PaperAuthors.FirstOrDefault()?.Author;
                    if (firstAuthor != null)
                    {
                        schedule.PresenterId = firstAuthor.UserId;
                    }
                }
            }

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<List<Schedule>> GetSchedulesByTimelineIdAsync(int timelineId)
        {
            return await _context.Schedules
                .Where(s => s.TimeLineId == timelineId)
                .Include(s => s.Paper)
                .Include(s => s.Conference)
                .Include(s => s.Presenter)
                .OrderBy(s => s.PresentationStartTime)
                .ToListAsync();
        }

        public async Task<int> CountSchedulesByTimelineIdAsync(int timelineId)
        {
            return await _context.Schedules
                .CountAsync(s => s.TimeLineId == timelineId);
        }
    }
}
