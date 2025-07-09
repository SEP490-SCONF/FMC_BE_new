namespace ConferenceFWebAPI.Service
{
    public class HangfireReminderService
    {
        private readonly ILogger<HangfireReminderService> _logger;

        public HangfireReminderService(ILogger<HangfireReminderService> logger)
        {
            _logger = logger;
        }

        // Phương thức này sẽ được Hangfire gọi
        public async Task SendTimeLineReminder(int timeLineId, string description)
        {
            // Đây là nơi bạn sẽ thêm logic gửi thông báo nhắc nhở thực tế
            // Ví dụ: Gửi email, push notification, SMS, hoặc ghi log.
            _logger.LogInformation($"--- REMINDER FOR TIME LINE ID: {timeLineId} ---");
            _logger.LogInformation($"Description: {description}");
            _logger.LogInformation($"Time: {DateTime.Now}");
            _logger.LogInformation("-------------------------------------");

            // Trong một ứng dụng thực tế, bạn có thể inject các dịch vụ
            // gửi thông báo ở đây và gọi chúng.
            // Ví dụ: await _notificationService.SendNotification(userId, description);

            await Task.CompletedTask; // Giả lập một tác vụ bất đồng bộ
        }
    }
}
