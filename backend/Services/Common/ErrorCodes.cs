namespace backend.Services.Common
{
    /// <summary>
    /// Machine-readable reasons a request was refused, returned as
    /// <c>{ code, message }</c>. The client translates the code — see "errors"
    /// in the Angular i18n files — so the reason reads in the user's language;
    /// the English message stays alongside it as a fallback.
    /// </summary>
    public static class ErrorCodes
    {
        public const string RoleNotFound = "role_not_found";
        public const string TelegramIdTaken = "telegram_id_taken";
        public const string UnsupportedLanguage = "unsupported_language";
        public const string UserNotFound = "user_not_found";
        public const string AdventureNotFound = "adventure_not_found";
        public const string ImageRequired = "image_required";
        public const string TooManyImages = "too_many_images";
        public const string ImageTooLarge = "image_too_large";
        public const string ImageTypeNotAllowed = "image_type_not_allowed";
        public const string SaveFailed = "save_failed";
    }
}
