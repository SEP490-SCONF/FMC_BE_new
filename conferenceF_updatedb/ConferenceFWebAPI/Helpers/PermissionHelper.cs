using System.Security.Claims;
using Repository;

namespace ConferenceFWebAPI.Helpers
{
    public static class PermissionHelper
    {
        /// <summary>
        /// Check if current user has organizer permission (ConferenceRoleId = 4) for specified conference
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from controller</param>
        /// <param name="userConferenceRoleRepository">Repository to check user roles</param>
        /// <param name="conferenceId">Conference ID to check permission for</param>
        /// <returns>True if user has organizer permission, false otherwise</returns>
        public static async Task<bool> HasOrganizerPermission(
            ClaimsPrincipal user, 
            IUserConferenceRoleRepository userConferenceRoleRepository, 
            int conferenceId)
        {
            // Get current user ID from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return false;

            // Check if user has organizer role (ConferenceRoleId = 4) for this conference
            var userRoles = await userConferenceRoleRepository.GetAll();
            return userRoles.Any(ucr => 
                ucr.UserId == userId && 
                ucr.ConferenceId == conferenceId && 
                ucr.ConferenceRoleId == 4);
        }

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from controller</param>
        /// <returns>User ID if found and valid, null otherwise</returns>
        public static int? GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }
    }
}
