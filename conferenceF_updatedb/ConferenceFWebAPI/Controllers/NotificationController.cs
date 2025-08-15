using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.Hubs;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationController(INotificationRepository notificationRepository, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpPost("test")]
        public async Task<IActionResult> AddTestNotification(int userId, string roleTarget = "Reviewer")
        {
            try
            {
                var notification = new Notification
                {
                    Title = "Thông báo kiểm tra",
                    Content = $"Đây là một thông báo thử nghiệm cho người dùng có ID: {userId}.",
                    UserId = userId,
                    RoleTarget = roleTarget,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddNotificationAsync(notification);

                return Ok(new { Message = "Thông báo đã được thêm thành công vào cơ sở dữ liệu.", Notification = notification });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Có lỗi xảy ra: {ex.Message}");
            }
        }
        [HttpPost("signalr-test")]
        public async Task<IActionResult> SendTestSignalR(int userId)
        {
            try
            {
                string title = "📡 Test SignalR";
                string content = $"🔔 Gửi thông báo real-time lúc {DateTime.UtcNow:HH:mm:ss}";

                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", title, content);

                return Ok(new
                {
                    Message = "✅ Đã gửi thông báo real-time SignalR thành công.",
                    TargetUser = userId,
                    SentAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Lỗi khi gửi SignalR: {ex.Message}");
            }
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationByUserId(int userId)
        {
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);

            if (notifications == null || !notifications.Any())
            {
                return NotFound($"Không tìm thấy thông báo nào cho người dùng có ID: {userId}.");
            }

            // Sử dụng AutoMapper để chuyển đổi từ Notification sang NotificationDto
            var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

            return Ok(notificationDtos);
        }
    }
}

