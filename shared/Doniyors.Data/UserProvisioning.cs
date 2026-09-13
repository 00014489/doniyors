using Doniyors.Data.Entities;

namespace Doniyors.Data
{
    /// <summary>
    /// The single rule for turning a Telegram identity into a row in
    /// <c>Users</c>.
    /// <para>
    /// Both services see the same people: someone can arrive through the bot,
    /// through the Mini App, or through both. When each service had its own
    /// copy of this logic they disagreed — one wrote the literal string
    /// "unknown" as a name, only one stored the first name, and one reset the
    /// member's chosen language on every <c>/start</c>. Keeping it here means
    /// there is one answer rather than two.
    /// </para>
    /// </summary>
    public static class UserProvisioning
    {
        /// <summary>Seeded account types; see the TypeUsers seed data.</summary>
        public const int SuperAdminRoleId = 1;
        public const int AdminRoleId = 2;
        public const int MemberRoleId = 3;

        /// <summary>A brand new member, with a freshly generated QR token.</summary>
        public static User Create(TelegramProfile profile, DateTimeOffset now)
        {
            var user = new User
            {
                TgUserId = profile.TgUserId,
                QrToken = TokenGenerator.Generate(),
                TypeUserId = MemberRoleId,
                Points = 0,
                Status = true,
                RegisteredAt = now,
                UpdatedAt = now,

                // Filled in by Apply below, so creation and refresh cannot
                // disagree about what a field should hold.
                UserName = string.Empty,
                FirstName = string.Empty,
                LanguageCode = string.Empty,
            };

            Apply(user, profile, now);

            return user;
        }

        /// <summary>
        /// Brings a stored member back in line with Telegram. Returns
        /// <c>true</c> when something actually changed, so the caller can skip
        /// a pointless write.
        /// </summary>
        public static bool Apply(User user, TelegramProfile profile, DateTimeOffset now)
        {
            var changed = false;

            // The @username is optional on Telegram. Store an empty string for
            // "not set" — never a placeholder like "unknown", which would show
            // up as somebody's name on the leaderboard.
            var userName = profile.UserName?.Trim() ?? string.Empty;

            if (user.UserName != userName)
            {
                user.UserName = userName;
                changed = true;
            }

            var firstName = profile.FirstName?.Trim() ?? string.Empty;

            if (user.FirstName != firstName)
            {
                user.FirstName = firstName;
                changed = true;
            }

            // Null means "this source cannot tell us" — only the Mini App
            // receives a picture — so it must not wipe a stored one.
            if (profile.PhotoUrl is not null && user.PhotoUrl != profile.PhotoUrl)
            {
                user.PhotoUrl = profile.PhotoUrl;
                changed = true;
            }

            // Telegram's locale only seeds the language, and only when the
            // product speaks it: a raw "de" or "en-US" in the column would be a
            // language no screen can show. Once a member has a supported one, it
            // is theirs to change — following the client locale here would
            // silently undo that choice on their next /start.
            var seeded = SupportedLanguages.Normalize(profile.LanguageCode);

            if (!SupportedLanguages.IsSupported(user.LanguageCode) && seeded is not null)
            {
                user.LanguageCode = seeded;
                changed = true;
            }

            if (changed)
            {
                user.UpdatedAt = now;
            }

            return changed;
        }
    }
}
