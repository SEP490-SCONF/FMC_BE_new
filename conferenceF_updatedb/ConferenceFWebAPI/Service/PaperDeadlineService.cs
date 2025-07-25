using Hangfire;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class PaperDeadlineService
    {
        private readonly IPaperRepository _paperRepository;
        private readonly ITimeLineRepository _timeLineRepository;

        public PaperDeadlineService(IPaperRepository paperRepository, ITimeLineRepository timeLineRepository)
        {
            _paperRepository = paperRepository;
            _timeLineRepository = timeLineRepository;
        }

        public async Task SchedulePaperReminders(int paperId)
        {
            // Lấy paper cùng với Conference và TimeLines từ repository
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId);

            if (paper == null || paper.Conference == null)
            {
                return;
            }

            // Lập lịch nhắc nhở cho thời hạn nộp bài
            var submissionDeadline = paper.Conference.TimeLines
                                          .FirstOrDefault(tl => tl.Description == "Submission Deadline" && tl.Date > DateTime.UtcNow);
            if (submissionDeadline != null)
            {
                string jobId = BackgroundJob.Schedule(() =>
                    SendSubmissionReminder(paperId, paper.Title),
                    submissionDeadline.Date);

                submissionDeadline.HangfireJobId = jobId;
                await _timeLineRepository.UpdateTimeLineAsync(submissionDeadline);
            }

            // Lập lịch nhắc nhở cho thời hạn đánh giá (ví dụ: 7 ngày trước thời hạn)
            var reviewDeadline = paper.Conference.TimeLines
                                        .FirstOrDefault(tl => tl.Description == "Review Deadline" && tl.Date > DateTime.UtcNow.AddDays(7));
            if (reviewDeadline != null)
            {
                string jobId = BackgroundJob.Schedule(() =>
                    SendReviewReminder(paperId, paper.Title),
                    reviewDeadline.Date.AddDays(-7)); // Lập lịch 7 ngày trước

                reviewDeadline.HangfireJobId = jobId;
                await _timeLineRepository.UpdateTimeLineAsync(reviewDeadline);
            }

            // Lập lịch nhắc nhở thanh toán phí xuất bản
            var publicationFeeDeadline = paper.Conference.TimeLines
                                              .FirstOrDefault(tl => tl.Description == "Publication Fee Deadline" && tl.Date > DateTime.UtcNow);
            if (publicationFeeDeadline != null)
            {
                string jobId = BackgroundJob.Schedule(() =>
                    SendPublicationFeeReminder(paperId, paper.Title),
                    publicationFeeDeadline.Date);

                publicationFeeDeadline.HangfireJobId = jobId;
                await _timeLineRepository.UpdateTimeLineAsync(publicationFeeDeadline);
            }
        }

        // --- Các phương thức Hangfire Job ---
        // Các phương thức này sẽ được Hangfire thực thi trong nền.
        // Chúng nên chứa logic thực tế để gửi thông báo (ví dụ: gửi email).

        public void SendSubmissionReminder(int paperId, string? paperTitle)
        {
            Console.WriteLine($"[Hangfire Job] Nhắc nhở: Thời hạn nộp bài '{paperTitle}' (ID: {paperId}) đang đến gần!");
            // TODO: Triển khai logic thông báo thực tế (ví dụ: gửi email cho các tác giả)
            // Để gửi email, bạn sẽ cần inject một EmailService ở đây và sử dụng nó.
        }

        public void SendReviewReminder(int paperId, string? paperTitle)
        {
            Console.WriteLine($"[Hangfire Job] Nhắc nhở: Thời hạn chấm bài '{paperTitle}' (ID: {paperId}) đang đến gần cho các reviewer!");
            // TODO: Triển khai logic thông báo thực tế (ví dụ: gửi email cho các reviewer được giao)
        }

        public void SendPublicationFeeReminder(int paperId, string? paperTitle)
        {
            Console.WriteLine($"[Hangfire Job] Nhắc nhở: Phí xuất bản bài báo '{paperTitle}' (ID: {paperId}) đến hạn thanh toán!");
            // TODO: Triển khai logic thông báo thực tế (ví dụ: gửi email cho các tác giả)
        }

        // Phương thức để hủy job nếu một dòng thời gian thay đổi hoặc một bài báo bị rút
        public void CancelScheduledReminder(string hangfireJobId)
        {
            if (!string.IsNullOrEmpty(hangfireJobId))
            {
                BackgroundJob.Delete(hangfireJobId);
                Console.WriteLine($"[Hangfire] Job {hangfireJobId} đã bị hủy.");
            }
        }
    }
}
