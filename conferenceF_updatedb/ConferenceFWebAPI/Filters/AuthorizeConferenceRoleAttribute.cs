using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceFWebAPI.Filters
{
    public class AuthorizeConferenceRoleAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public AuthorizeConferenceRoleAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Nếu chưa đăng nhập
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy conferenceId từ route (có thể là "id" hoặc "conferenceId")
            var conferenceId = context.RouteData.Values["id"]?.ToString()
                            ?? context.RouteData.Values["conferenceId"]?.ToString();

            if (conferenceId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Lấy toàn bộ claims ConferenceRole
            var roles = user.FindAll("ConferenceRole");

            // Debug nhanh: nếu cần log ra console
            // Console.WriteLine($"Claims: {string.Join(",", roles.Select(r => r.Value))}");

            // Check xem có claim "conferenceId:RoleName" khớp không
            var hasRole = roles.Any(r => r.Value == $"{conferenceId}:{_requiredRole}");

            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }
    
}

}
