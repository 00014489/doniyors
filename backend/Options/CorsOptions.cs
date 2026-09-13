namespace backend.Options
{
    /// <summary>
    /// Bound from the "Cors" section. Allowed origins are configuration, not
    /// code, so a new tunnel or domain does not need a rebuild.
    /// </summary>
    public sealed class CorsOptions
    {
        public const string SectionName = "Cors";

        public string[] AllowedOrigins { get; set; } = [];
    }
}
