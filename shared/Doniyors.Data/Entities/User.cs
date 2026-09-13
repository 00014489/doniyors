using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doniyors.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public long TgUserId { get; set; }

        /// <summary>Telegram @username. Empty for accounts that have never set one.</summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Telegram first name. Kept because <see cref="UserName"/> is optional
        /// on Telegram, and a leaderboard row needs something to show.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Telegram profile picture, refreshed on every Mini App sign-in.
        /// Null for a user who has only ever talked to the bot, or who has no
        /// picture — the client falls back to initials.
        /// </summary>
        public string? PhotoUrl { get; set; }

        public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public int Points { get; set; } = 0;

        public string QrToken { get; set; } = null!;
        public string LanguageCode { get; set; } = string.Empty;

        public int TypeUserId { get; set; } = 3;

        /// <summary>
        /// Soft-delete flag. <c>false</c> means the account is disabled but
        /// the row (and its history) is kept.
        /// </summary>
        public bool Status { get; set; } = true;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public TypeUser TypeUser { get; set; } = null!;
        public UserSession? Session { get; set; }
    }
}