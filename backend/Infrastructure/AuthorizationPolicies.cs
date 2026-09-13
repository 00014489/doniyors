namespace backend.Infrastructure
{
    /// <summary>Named policies, so controllers do not repeat role lists.</summary>
    public static class AuthorizationPolicies
    {
        /// <summary>Admin panel access: Admin or SuperAdmin.</summary>
        public const string Administrative = "Administrative";
    }
}
