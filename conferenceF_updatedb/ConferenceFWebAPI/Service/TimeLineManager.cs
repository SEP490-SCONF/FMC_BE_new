using Azure.Storage.Blobs;
using BussinessObject.Entity;
using Hangfire;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class TimeLineManager // Ví dụ về một class quản lý Timeline
    {
        private readonly ITimeLineRepository _timeLineRepository;
        private readonly IBackgroundJobClient _jobClient;
        private readonly HangfireReminderService _reminderService;
        // Có thể cần DbContext ở đây nếu repository không cung cấp đủ quyền truy cập cho update/delete chi tiết

        public TimeLineManager(ITimeLineRepository timeLineRepository, IBackgroundJobClient jobClient, HangfireReminderService reminderService)
        {
            _timeLineRepository = timeLineRepository;
            _jobClient = jobClient;
            _reminderService = reminderService;
        }

        // Tạo Timeline mới và lên lịch nhắc nhở
        public async Task<TimeLine> CreateTimelineWithReminderAsync(TimeLine newTimeLine)
        {
            // Bước 1: Lưu Timeline vào DB trước để có TimeLineId.
            // HangfireJobId ban đầu là null
            var createdTimeLine = await _timeLineRepository.CreateTimeLineAsync(newTimeLine);

            // Bước 2: Tính toán thời gian nhắc nhở (1 tuần trước Date)
            // Current time is Wednesday, July 9, 2025 at 10:12:24 PM +07.
            // Nếu newTimeLine.Date là 2025-07-16 10:00:00 AM, thì reminderTime sẽ là 2025-07-09 10:00:00 AM.
            // Điều này có nghĩa là nếu deadline là 1 tuần nữa, job sẽ được lên lịch ngay lập tức.
            // Nếu deadline sớm hơn 1 tuần, job sẽ được lên lịch trong quá khứ và chạy ngay.
            // Nên cân nhắc logic nếu Date quá gần hoặc đã qua
            DateTime reminderTime = createdTimeLine.Date.AddDays(-7);

            // Bước 3: Kiểm tra và lên lịch job
            string? hangfireJobId = null;
            if (reminderTime <= DateTime.UtcNow) // Dùng UTC để so sánh nhất quán
            {
                Console.WriteLine($"Cảnh báo: Ngày nhắc nhở ({reminderTime.ToLocalTime()}) cho Timeline ID {createdTimeLine.TimeLineId} đã qua hoặc quá gần hiện tại ({DateTime.Now.ToLocalTime()}). Gửi nhắc nhở ngay.");
                // Gửi nhắc nhở ngay lập tức nếu deadline đã quá gần hoặc đã qua
                _reminderService.SendTimelineReminder(createdTimeLine.TimeLineId, createdTimeLine.Description);
                // Không cần lên lịch Hangfire job nếu nó chạy ngay lập tức.
                // Tuy nhiên, nếu bạn muốn vẫn có bản ghi trong Hangfire Dashboard, bạn có thể Enqueue một job ngay lập tức.
                hangfireJobId = _jobClient.Enqueue(() => _reminderService.SendTimelineReminder(createdTimeLine.TimeLineId, createdTimeLine.Description));
            }
            else
            {
                // Lên lịch job với Hangfire
                hangfireJobId = _jobClient.Schedule(
                    () => _reminderService.SendTimelineReminder(createdTimeLine.TimeLineId, createdTimeLine.Description),
                    reminderTime
                );
                Console.WriteLine($"Đã lên lịch nhắc nhở cho Timeline ID {createdTimeLine.TimeLineId} vào lúc {reminderTime.ToLocalTime()}. Hangfire Job ID: {hangfireJobId}");
            }

            // Bước 4: Cập nhật lại entity trong DB với HangfireJobId
            if (hangfireJobId != null)
            {
                createdTimeLine.HangfireJobId = hangfireJobId;
                await _timeLineRepository.UpdateTimeLineAsync(createdTimeLine); // Lưu HangfireJobId vào DB
            }

            return createdTimeLine;
        }

        // Cập nhật Timeline và lên lịch lại nhắc nhở
        public async Task<TimeLine?> UpdateTimelineWithReminderAsync(int timelineId, TimeLine updatedTimeLineData)
        {
            var existingTimeLine = await _timeLineRepository.GetTimeLineByIdAsync(timelineId);

            if (existingTimeLine == null)
            {
                return null; // Không tìm thấy timeline để cập nhật
            }

            // Bước 1: Xóa job cũ nếu có (quan trọng khi cập nhật timeline)
            if (!string.IsNullOrEmpty(existingTimeLine.HangfireJobId))
            {
                _jobClient.Delete(existingTimeLine.HangfireJobId);
                Console.WriteLine($"Đã xóa Hangfire Job ID cũ: {existingTimeLine.HangfireJobId}");
            }

            // Bước 2: Cập nhật các thuộc tính của existingTimeLine từ updatedTimeLineData
            existingTimeLine.Date = updatedTimeLineData.Date;
            existingTimeLine.Description = updatedTimeLineData.Description;
            // existingTimeLine.ConferenceId = updatedTimeLineData.ConferenceId; // Cập nhật ConferenceId nếu cần

            // Bước 3: Tính toán và lên lịch job mới
            DateTime reminderTime = existingTimeLine.Date.AddDays(-7);

            string? newHangfireJobId = null;
            if (reminderTime <= DateTime.UtcNow) // Dùng UTC để so sánh nhất quán
            {
                Console.WriteLine($"Cảnh báo: Ngày nhắc nhở ({reminderTime.ToLocalTime()}) cho Timeline ID {existingTimeLine.TimeLineId} đã qua hoặc quá gần. Gửi nhắc nhở ngay.");
                _reminderService.SendTimelineReminder(existingTimeLine.TimeLineId, existingTimeLine.Description);
                newHangfireJobId = _jobClient.Enqueue(() => _reminderService.SendTimelineReminder(existingTimeLine.TimeLineId, existingTimeLine.Description));
            }
            else
            {
                newHangfireJobId = _jobClient.Schedule(
                    () => _reminderService.SendTimelineReminder(existingTimeLine.TimeLineId, existingTimeLine.Description),
                    reminderTime
                );
                Console.WriteLine($"Đã lên lịch lại nhắc nhở cho Timeline ID {existingTimeLine.TimeLineId} vào lúc {reminderTime.ToLocalTime()}. Hangfire Job ID: {newHangfireJobId}");
            }

            // Bước 4: Lưu JobId mới vào entity và cập nhật DB
            existingTimeLine.HangfireJobId = newHangfireJobId;
            var success = await _timeLineRepository.UpdateTimeLineAsync(existingTimeLine);

            if (!success)
            {
                // Xử lý lỗi nếu không cập nhật được DB
                return null;
            }

            return existingTimeLine;
        }

        // Xóa Timeline và job nhắc nhở liên quan
        public async Task<bool> DeleteTimelineAndReminderAsync(int timelineId)
        {
            var timelineToDelete = await _timeLineRepository.GetTimeLineByIdAsync(timelineId);
            if (timelineToDelete == null)
            {
                return false; // Không tìm thấy timeline
            }

            // Xóa job Hangfire trước
            if (!string.IsNullOrEmpty(timelineToDelete.HangfireJobId))
            {
                _jobClient.Delete(timelineToDelete.HangfireJobId);
                Console.WriteLine($"Đã xóa Hangfire Job ID: {timelineToDelete.HangfireJobId} liên quan đến Timeline ID {timelineId}");
            }

            // Sau đó xóa timeline khỏi DB
            var success = await _timeLineRepository.DeleteTimeLineAsync(timelineId); // Giả sử repo có phương thức Delete
            return success;
        }

        // Lấy Timeline theo ID
        public async Task<TimeLine?> GetTimelineByIdAsync(int id)
        {
            return await _timeLineRepository.GetTimeLineByIdAsync(id);
        }

        // Lấy TimeLines theo ConferenceId
        public async Task<IEnumerable<TimeLine>> GetTimeLinesByConferenceAsync(int conferenceId)
        {
            return await _timeLineRepository.GetTimeLinesByConferenceAsync(conferenceId);
        }
    }
}