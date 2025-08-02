using Microsoft.AspNetCore.SignalR;

namespace ConferenceFWebAPI.Hubs
{
    public class TimelineReminderHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
