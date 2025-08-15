// Repository/TimeLineRepository.cs
using BussinessObject.Entity;
using DataAccess; // Import DAO namespace
using Microsoft.EntityFrameworkCore;
using Repository.Repository; // Import Interface Repository
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository // Điều chỉnh namespace
{
    public class TimeLineRepository : ITimeLineRepository
    {
        private readonly TimeLineDAO _timeLineDAO;

        public TimeLineRepository(TimeLineDAO timeLineDAO) // Chỉ inject TimeLineDAO
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
            await _timeLineDAO.AddAsync(timeLine);
            await _timeLineDAO.SaveChangesAsync(); // Repository chịu trách nhiệm lưu thay đổi
            return timeLine;
        }

        public async Task<bool> UpdateTimeLineAsync(TimeLine timeLine)
        {
            var existingTimeLine = await _timeLineDAO.GetByIdAsync(timeLine.TimeLineId);
            if (existingTimeLine == null)
            {
                return false;
            }

            // Cập nhật các thuộc tính của existingTimeLine
            existingTimeLine.Date = timeLine.Date;
            existingTimeLine.Description = timeLine.Description;
            existingTimeLine.HangfireJobId = timeLine.HangfireJobId;
            existingTimeLine.ConferenceId = timeLine.ConferenceId; // Đảm bảo ID hội nghị được giữ nguyên

            _timeLineDAO.Update(existingTimeLine); // Gọi phương thức Update (synchronous) trên DAO
            await _timeLineDAO.SaveChangesAsync(); // Repository chịu trách nhiệm lưu thay đổi
            return true;
        }

        public async Task<bool> DeleteTimeLineAsync(int id)
        {
            var timeLine = await _timeLineDAO.GetByIdAsync(id); // Lấy entity thông qua DAO
            if (timeLine == null)
            {
                return false;
            }

            _timeLineDAO.Delete(timeLine); // Gọi phương thức Delete trên DAO
            await _timeLineDAO.SaveChangesAsync(); // Repository chịu trách nhiệm lưu thay đổi
            return true;
        }
    }
}