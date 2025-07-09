﻿using BussinessObject.Entity;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repository
{
    public class TimeLineRepository : ITimeLineRepository
    {
        private readonly TimeLineDAO _timeLineDAO;
        private readonly ConferenceFTestContext _context; // Đảm bảo đã inject DbContext

        public TimeLineRepository(TimeLineDAO timeLineDAO)
        {
            _timeLineDAO = timeLineDAO;
        }

        public async Task<List<TimeLine>> GetTimeLinesByConferenceAsync(int conferenceId)
        {
            return await _timeLineDAO.GetByConferenceIdAsync(conferenceId);
        }

        public async Task<TimeLine?> GetTimeLineByIdAsync(int id)
        {
            return await _timeLineDAO.GetByIdAsync(id);
        }

        public async Task<TimeLine> CreateTimeLineAsync(TimeLine timeLine)
        {
            return await _timeLineDAO.AddAsync(timeLine);
        }

        public async Task<bool> UpdateTimeLineAsync(TimeLine timeLine)
        {
            // Kiểm tra sự tồn tại trước khi cập nhật
            var existingTimeLine = await _timeLineDAO.GetByIdAsync(timeLine.TimeLineId);
            if (existingTimeLine == null)
            {
                return false;
            }

            existingTimeLine.Date = timeLine.Date;
            existingTimeLine.Description = timeLine.Description;
            // Quan trọng: Gán lại HangfireJobId nếu nó bị thay đổi
            existingTimeLine.HangfireJobId = timeLine.HangfireJobId;

            await _timeLineDAO.UpdateAsync(existingTimeLine);
            return true;
        }
        public async Task<bool> DeleteTimeLineAsync(int id)
        {
            var timeLine = await _context.TimeLines.FindAsync(id);
            if (timeLine == null)
            {
                return false; // Không tìm thấy timeline để xóa
            }

            _context.TimeLines.Remove(timeLine);
            await _context.SaveChangesAsync();
            return true; // Xóa thành công
        }
    }
}
