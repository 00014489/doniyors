namespace backend.DTOs.Member
{
    /// <summary>One adventure the member has taken part in.</summary>
    public class ProfileHistoryItemDto
    {
        /// <summary>The transaction that recorded it — unique per row.</summary>
        public int TransactionId { get; set; }

        public int? AdventureId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset Date { get; set; }

        public int? CoverImageId { get; set; }

        /// <summary>Points this entry awarded.</summary>
        public int Points { get; set; }
    }

    public class ProfileDto
    {
        public long TgUserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Telegram @username, without the @. Empty when unset.</summary>
        public string UserName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        /// <summary>Current balance, the sum of all active transactions.</summary>
        public int Points { get; set; }

        /// <summary>
        /// Adventures the member has been credited for: active transactions
        /// that both added points and name an adventure.
        /// </summary>
        public int TravelsCount { get; set; }

        public DateTimeOffset RegisteredAt { get; set; }

        /// <summary>Adventures attended, most recent first.</summary>
        public IReadOnlyList<ProfileHistoryItemDto> History { get; set; } = [];
    }
}
