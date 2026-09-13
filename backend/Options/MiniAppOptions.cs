using System.ComponentModel.DataAnnotations;

namespace backend.Options
{
    /// <summary>
    /// Bound from the "MiniApp" section. Optional: without <see cref="Url"/> the
    /// API leaves the bot's menu button alone when a language changes.
    /// </summary>
    public sealed class MiniAppOptions
    {
        public const string SectionName = "MiniApp";

        /// <summary>Public HTTPS address of the Angular app.</summary>
        [Url(ErrorMessage = "MiniApp:Url must be an absolute URL.")]
        public string? Url { get; set; }
    }
}
