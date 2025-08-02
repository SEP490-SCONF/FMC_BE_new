using ConferenceFWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ConferenceFWebAPI.Service
{
    public class HangfireReminderService
    {
        // Phương thức này sẽ được Hangfire gọi
        public void SendTimelineReminder(int timelineId, string description)
        {
            // Đây là nơi bạn thực hiện logic gửi nhắc nhở thực tế
            // Ví dụ: gửi email, gửi thông báo push, log ra console, v.v.

            Console.WriteLine($"--- Nhắc nhở Timeline ---");
            Console.WriteLine($"Timeline ID: {timelineId}");
            Console.WriteLine($"Nội dung: {description}");
            Console.WriteLine($"Thời gian: {DateTime.Now}");
            Console.WriteLine($"--------------------------");

            // Có thể thêm logic gọi API gửi email, v.v. ở đây
            // Ví dụ: await _emailSender.SendEmailAsync("user@example.com", "Nhắc nhở Timeline", description);
        }
    }
}
