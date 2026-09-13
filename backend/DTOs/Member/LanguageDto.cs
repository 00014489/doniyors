namespace backend.DTOs.Member
{
    /// <summary>A member's language, as stored in <c>Users.LanguageCode</c>.</summary>
    public class LanguageDto
    {
        /// <summary>"uz", "ru" or "en"; empty when the member has not chosen yet.</summary>
        public string LanguageCode { get; set; } = string.Empty;
    }
}
