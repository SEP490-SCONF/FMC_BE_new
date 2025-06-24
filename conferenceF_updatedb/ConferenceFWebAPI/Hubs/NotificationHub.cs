using Microsoft.AspNetCore.SignalR;

namespace ConferenceFWebAPI.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task MarkAsRead(int notificationId)
        {
            // Xử lý logic cập nhật trạng thái trong database...
            // Sau đó có thể thông báo lại cho client rằng đã cập nhật thành công
            await Clients.Caller.SendAsync("MarkAsReadSuccess", notificationId);
        }
    }
}
