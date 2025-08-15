using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ConferenceFWebAPI.Provider
{
    

    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Lấy claim NameIdentifier (userId bạn đã set trong JWT)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

}
