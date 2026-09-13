namespace backend.Services.JwtService
{
    /// <summary>
    /// Claim names as they appear in the token payload. They are short on
    /// purpose: inbound claim mapping is switched off, so what is written here
    /// is exactly what <c>User.FindFirst</c> reads back.
    /// </summary>
    public static class ClaimNames
    {
        public const string UserId = "UserId";
        public const string TypeUser = "TypeUser";
        public const string Role = "role";
    }


    /// <summary>Role names used by <c>[Authorize(Roles = ...)]</c>.</summary>
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string User = "User";

        /// <summary>Both roles that may use the admin panel.</summary>
        public const string Administrative = $"{Admin},{SuperAdmin}";
    }
}
