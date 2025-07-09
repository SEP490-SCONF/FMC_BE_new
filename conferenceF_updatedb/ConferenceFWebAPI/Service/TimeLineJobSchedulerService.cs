using Hangfire;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class TimeLineJobSchedulerService : BackgroundService
    {
        private readonly ILogger<TimeLineJobSchedulerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TimeLineJobSchedulerService(ILogger<TimeLineJobSchedulerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TimeLine Job Scheduler Service running.");

            // Initial delay to give the application time to fully start and Hangfire to initialize
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Added a small initial delay

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("TimeLine Job Scheduler Service checking for updates...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var timeLineRepository = scope.ServiceProvider.GetRequiredService<ITimeLineRepository>();
                    var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
                    var hangfireReminderService = scope.ServiceProvider.GetRequiredService<HangfireReminderService>();

                    // Lấy tất cả timeline. Cân nhắc phân trang nếu số lượng lớn.
                    var allTimeLines = await timeLineRepository.GetTimeLinesByConferenceAsync(0);

                    foreach (var timeLine in allTimeLines)
                    {
                        var nowUtc = DateTime.UtcNow;

                        // 1. Xử lý các timeline có Date đã qua
                        if (timeLine.Date <= nowUtc)
                        {
                            if (!string.IsNullOrEmpty(timeLine.HangfireJobId))
                            {
                                backgroundJobClient.Delete(timeLine.HangfireJobId);
                                _logger.LogInformation($"Deleted past Hangfire job {timeLine.HangfireJobId} for TimeLine ID: {timeLine.TimeLineId}.");

                                // Reset HangfireJobId trong DB
                                timeLine.HangfireJobId = null;
                                await timeLineRepository.UpdateTimeLineAsync(timeLine);
                            }
                            continue; // Bỏ qua timeline này, không cần lên lịch
                        }

                        // 2. Xử lý các timeline có Date trong tương lai
                        // Logic đơn giản: Nếu HangfireJobId rỗng hoặc bị reset (từ Controller), hoặc job cũ không tồn tại
                        // thì chúng ta cần lên lịch mới.
                        // Để đảm bảo job luôn được cập nhật, chúng ta sẽ luôn tạo lại job nếu HangfireJobId là null.
                        // Khi Controller update, nó sẽ set HangfireJobId về null, kích hoạt việc tạo job mới.

                        bool needsNewJob = string.IsNullOrEmpty(timeLine.HangfireJobId);

                        // Nếu JobId tồn tại, kiểm tra xem nó có còn hợp lệ trong Hangfire không.
                        // (Dù GetStateData không dùng được, nhưng GetStateDataFromJobId (hoặc tương tự từ Monitoring API)
                        // có thể kiểm tra sự tồn tại. Tuy nhiên, cách đơn giản nhất là xóa và tạo lại.)
                        // Nếu bạn muốn giữ job cũ nếu Date không đổi, bạn sẽ cần một trường LastScheduledDate trong entity.
                        // Để đơn giản và mạnh mẽ: nếu JobId null -> tạo mới. Nếu JobId có -> xóa cũ và tạo mới.

                        if (!string.IsNullOrEmpty(timeLine.HangfireJobId))
                        {
                            // Trước khi tạo job mới, xóa job cũ để tránh trùng lặp
                            // và đảm bảo thông tin job luôn mới nhất.
                            // IBackgroundJobClient.Delete sẽ không ném lỗi nếu job không tồn tại.
                            backgroundJobClient.Delete(timeLine.HangfireJobId);
                            _logger.LogInformation($"Deleted existing Hangfire job {timeLine.HangfireJobId} for TimeLine ID: {timeLine.TimeLineId}.");
                            timeLine.HangfireJobId = null; // Reset để logic dưới tạo job mới
                        }

                        // Lên lịch job mới
                        var delay = timeLine.Date - nowUtc;
                        if (delay.TotalSeconds > 0) // Đảm bảo thời gian còn trong tương lai
                        {
                            var jobId = backgroundJobClient.Schedule(
                                () => hangfireReminderService.SendTimeLineReminder(timeLine.TimeLineId, timeLine.Description),
                                delay
                            );

                            timeLine.HangfireJobId = jobId;
                            await timeLineRepository.UpdateTimeLineAsync(timeLine); // Cập nhật Job ID vào database
                            _logger.LogInformation($"Scheduled new Hangfire job {jobId} for TimeLine ID: {timeLine.TimeLineId} at {timeLine.Date.ToLocalTime()} (UTC: {timeLine.Date}).");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Kiểm tra mỗi 5 phút
            }

            _logger.LogInformation("TimeLine Job Scheduler Service stopped.");
        }
    }
}
