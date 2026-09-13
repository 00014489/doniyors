using System.Security.Claims;
using System.Text;
using Doniyors.Data;
using Doniyors.Data.Entities;
using backend.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.JwtService
{
    public class Jwt : IJwt
    {
        /// <summary>
        /// The modern handler. <c>JwtSecurityTokenHandler</c> is the legacy
        /// one — it rewrites short claim names into long URIs, which the
        /// Angular client would then have to know about.
        /// </summary>
        private static readonly JsonWebTokenHandler Handler = new();

        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;
        private readonly ILogger<Jwt> _logger;

        public Jwt(IOptions<JwtOptions> options, ILogger<Jwt> logger)
        {
            _options = options.Value;
            _logger = logger;

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Key));

            _credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);
        }

        public string Create(User user)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
                SigningCredentials = _credentials,

                // Written to the payload verbatim. "UserId" and "TypeUser" are
                // the names the API controllers and the Angular client read;
                // "role" drives [Authorize(Roles = ...)].
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
                    ["UserId"] = user.Id.ToString(),
                    ["TypeUser"] = user.TypeUserId.ToString(),
                    [ClaimNames.Role] = ResolveRoleName(user),
                },
            };

            return Handler.CreateToken(descriptor);
        }

        /// <summary>
        /// Prefers the loaded navigation property. Callers that forget to
        /// <c>Include</c> it used to mint a token with an empty role, which
        /// locks an administrator out of every admin endpoint with no error to
        /// explain it — so fall back to the seeded ids instead.
        /// </summary>
        private string ResolveRoleName(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.TypeUser?.Name))
                return user.TypeUser.Name;

            var fallback = user.TypeUserId switch
            {
                UserProvisioning.SuperAdminRoleId => "SuperAdmin",
                UserProvisioning.AdminRoleId => "Admin",
                UserProvisioning.MemberRoleId => "User",
                _ => string.Empty,
            };

            _logger.LogWarning(
                "TypeUser navigation was not loaded for user {UserId}; "
                + "resolved role {Role} from TypeUserId {TypeUserId}.",
                user.Id,
                string.IsNullOrEmpty(fallback) ? "<unknown>" : fallback,
                user.TypeUserId);

            return fallback;
        }
    }
}
