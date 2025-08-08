using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;
using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace ConferenceFWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationController(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
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

