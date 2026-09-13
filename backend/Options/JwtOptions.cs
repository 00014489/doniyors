using System.ComponentModel.DataAnnotations;

namespace backend.Options
{
    /// <summary>
    /// Bound from the "Jwt" section. In containers these arrive as the
    /// <c>Jwt__Key</c>, <c>Jwt__Issuer</c>, … environment variables.
    /// </summary>
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        /// <summary>
        /// HMAC-SHA256 signing key. 32 bytes is the algorithm's minimum — a
        /// shorter key makes token creation throw at the first login rather
        /// than at startup, so it is checked here instead.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Jwt:Key is required.")]
        [MinLength(32, ErrorMessage = "Jwt:Key must be at least 32 characters for HMAC-SHA256.")]
        public string Key { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Issuer { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 60 * 24 * 30)]
        public int ExpiresMinutes { get; set; } = 60;
    }
}
