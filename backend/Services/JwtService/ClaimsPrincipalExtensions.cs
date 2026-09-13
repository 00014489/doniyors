using System.Security.Claims;

namespace backend.Services.JwtService
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Reads the "UserId" claim minted by <see cref="Jwt"/>. False when the
        /// claim is absent or not a number, which the caller answers with 401.
        /// </summary>
        public static bool TryGetUserId(this ClaimsPrincipal principal, out int userId)
        {
            userId = 0;

            var claim = principal.FindFirst(ClaimNames.UserId)?.Value;

            return claim is not null && int.TryParse(claim, out userId);
        }
    }
}
