using ConferenceFWebAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class NotificationsController : ControllerBase
        {
            private readonly NotificationService _notificationService;

            public NotificationsController(NotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            /// <summary>
            /// Gửi thông báo đến một người dùng cụ thể.
            /// </summary>
            [HttpPost("send-to-user")]
            public async Task<IActionResult> SendNotificationToUser([FromQuery] int userId, [FromQuery] string title, [FromQuery] string content)
            {
                if (userId <= 0 || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    return BadRequest("UserId, Title, and Content are required.");
                }

                await _notificationService.SendNotificationToUserAsync(userId, title, content);
                return Ok($"Notification sent to user {userId}.");
            }

            /// <summary>
            /// Gửi thông báo đến tất cả người dùng có một vai trò cụ thể trong một hội thảo.
            /// </summary>
            [HttpPost("send-to-role")]
            public async Task<IActionResult> SendNotificationToRole([FromQuery] int conferenceId, [FromQuery] string roleName, [FromQuery] string title, [FromQuery] string content)
            {
                if (conferenceId <= 0 || string.IsNullOrEmpty(roleName) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    return BadRequest("ConferenceId, RoleName, Title, and Content are required.");
                }

                await _notificationService.SendNotificationToRoleAsync(conferenceId, roleName, title, content);
                return Ok($"Notification sent to role '{roleName}' in conference {conferenceId}.");
            }

            // Bạn có thể thêm các endpoint khác nếu cần, ví dụ để lấy danh sách thông báo đã lưu
        }
    }

