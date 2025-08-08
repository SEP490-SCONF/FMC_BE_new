using BussinessObject.Entity;
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

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
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
    }
}

