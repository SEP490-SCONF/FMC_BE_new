using BussinessObject.Entity;
using ConferenceFWebAPI.Hubs;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Repository;

namespace ConferenceFWebAPI.Service
{
    public class PaperDeadlineService : IPaperDeadlineService
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly INotificationRepository _notificationRepository; // Add this repository
                                                                          // If you are using SignalR, also inject the hub context:
        private readonly IHubContext<NotificationHub> _hubContext;

        public PaperDeadlineService(
            IPaperRepository paperRepository,
            IBackgroundJobClient backgroundJobClient,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext) // Add the repository to the constructor

        {
            _paperRepository = paperRepository;
            _backgroundJobClient = backgroundJobClient;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // --- Hàm lên lịch không thay đổi, chỉ cần đảm bảo nó gọi đúng các hàm dưới đây ---
        public async Task SchedulePaperReminders(int paperId)
        {
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId); // Use this method to get conference deadlines
            if (paper == null || !paper.SubmitDate.HasValue)
            {
                Console.WriteLine($"Error: Paper with ID {paperId} not found or SubmitDate is null.");
                return;
            }

            // Lấy các giá trị offset từ Conference.TimeLines
            var reviewOffsetDays = 14; // Số ngày cho hạn review sau SubmitDate
            var paymentOffsetDays = 30; // Số ngày cho hạn thanh toán sau SubmitDate (nếu accepted)
            var resubmissionOffsetDays = 21; // Số ngày cho hạn nộp lại sau SubmitDate (nếu need revision)
            var reviewDeadline = paper.SubmitDate.Value.AddDays(reviewOffsetDays);
            var paymentDeadline = paper.SubmitDate.Value.AddDays(paymentOffsetDays);
            var resubmissionDeadline = paper.SubmitDate.Value.AddDays(resubmissionOffsetDays);

            Console.WriteLine($"Scheduling reminders for Paper ID: {paper.PaperId}");

            _backgroundJobClient.Schedule<PaperDeadlineService>(
                x => x.RemindReviewers(paper.PaperId),
                reviewDeadline
            );

            _backgroundJobClient.Schedule<PaperDeadlineService>(
                x => x.RemindAuthorsForPayment(paper.PaperId),
                paymentDeadline
            );

            _backgroundJobClient.Schedule<PaperDeadlineService>(
                x => x.RemindAuthorsForResubmission(paper.PaperId),
                resubmissionDeadline
            );
        }

        // --- Các phương thức được Hangfire gọi (ĐÃ THÊM LOGIC LƯU VÀO NOTIFICATION) ---

        public async Task RemindReviewers(int paperId)
        {
            // Lấy bài báo cùng với ReviewerAssignments và Reviewer (thông qua PaperDAO và Repository)
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId);
            if (paper == null || paper.Status != "Under Review") return;

            Console.WriteLine($"Executing RemindReviewers for Paper ID: {paperId}, Status: {paper.Status}");

            var reviewerAssignments = paper.ReviewerAssignments;
            if (reviewerAssignments != null && reviewerAssignments.Any())
            {
                foreach (var assignment in reviewerAssignments)
                {
                    var reviewerUser = assignment.Reviewer?.UserConferenceRoles.FirstOrDefault()?.User;
                    if (reviewerUser != null)
                    {
                        string title = "Nhắc nhở: Hạn chót đánh giá bài báo sắp đến";
                        string content = $"Bài báo '{paper.Title}' cần bạn đánh giá. Hạn chót sắp đến!";
                        Console.WriteLine($"[Hangfire] Sending review reminder to Reviewer '{reviewerUser.Name}' (ID: {reviewerUser.UserId}) for Paper '{paper.Title}'.");

                        // Tạo và lưu thông báo vào cơ sở dữ liệu
                        var notification = new Notification
                        {
                            Title = title,
                            Content = content,
                            UserId = reviewerUser.UserId,
                            RoleTarget = "Reviewer",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddNotificationAsync(notification);
                        // If using SignalR, you would send the real-time notification here as well
                        await _hubContext.Clients.User(reviewerUser.UserId.ToString()).SendAsync("ReceiveNotification", title, content);
                    }
                    else
                    {
                        Console.WriteLine($"[Hangfire] Could not find User for Reviewer Assignment ID: {assignment.ReviewerId}.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[Hangfire] No reviewer assignments found for Paper ID: {paperId}.");
            }
        }

        public async Task RemindAuthorsForPayment(int paperId)
        {
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId);
            if (paper == null || paper.Status != "Accepted") return;

            Console.WriteLine($"Executing RemindAuthorsForPayment for Paper ID: {paperId}, Status: {paper.Status}");

            var paperAuthors = paper.PaperAuthors;
            if (paperAuthors != null && paperAuthors.Any())
            {
                foreach (var pa in paperAuthors)
                {
                    var authorUser = pa.Author?.UserConferenceRoles.FirstOrDefault()?.User;
                    if (authorUser != null)
                    {
                        string title = "Nhắc nhở: Phí xuất bản đến hạn";
                        string content = $"Bài báo '{paper.Title}' của bạn đã được chấp nhận. Vui lòng thanh toán phí xuất bản!";
                        Console.WriteLine($"[Hangfire] Sending payment reminder to Author '{authorUser.Name}' (ID: {authorUser.UserId}) for Paper '{paper.Title}'.");

                        // Tạo và lưu thông báo vào cơ sở dữ liệu
                        var notification = new Notification
                        {
                            Title = title,
                            Content = content,
                            UserId = authorUser.UserId,
                            RoleTarget = "Author",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddNotificationAsync(notification);
                        await _hubContext.Clients.User(authorUser.UserId.ToString()).SendAsync("ReceiveNotification", title, content);
                    }
                    else
                    {
                        Console.WriteLine($"[Hangfire] Could not find User for PaperAuthor ID: {pa.AuthorId}.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[Hangfire] No authors found for Paper ID: {paperId}.");
            }
        }

        public async Task RemindAuthorsForResubmission(int paperId)
        {
            var paper = await _paperRepository.GetPaperWithConferenceAndTimelinesAsync(paperId);
            if (paper == null || paper.Status != "Need Revision") return;

            Console.WriteLine($"Executing RemindAuthorsForResubmission for Paper ID: {paperId}, Status: {paper.Status}");

            var paperAuthors = paper.PaperAuthors;
            if (paperAuthors != null && paperAuthors.Any())
            {
                foreach (var pa in paperAuthors)
                {
                    var authorUser = pa.Author?.UserConferenceRoles.FirstOrDefault()?.User;
                    if (authorUser != null)
                    {
                        string title = "Nhắc nhở: Hạn chót nộp lại bài báo sắp đến";
                        string content = $"Bài báo '{paper.Title}' của bạn cần sửa đổi. Vui lòng nộp lại trước hạn chót!";
                        Console.WriteLine($"[Hangfire] Sending resubmission reminder to Author '{authorUser.Name}' (ID: {authorUser.UserId}) for Paper '{paper.Title}'.");

                        // Tạo và lưu thông báo vào cơ sở dữ liệu
                        var notification = new Notification
                        {
                            Title = title,
                            Content = content,
                            UserId = authorUser.UserId,
                            RoleTarget = "Author",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddNotificationAsync(notification);
                        await _hubContext.Clients.User(authorUser.UserId.ToString()).SendAsync("ReceiveNotification", title, content);
                    }
                    else
                    {
                        Console.WriteLine($"[Hangfire] Could not find User for PaperAuthor ID: {pa.AuthorId}.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[Hangfire] No authors found for Paper ID: {paperId}.");
            }
        }
    }
}
