namespace backend.DTOs.Member
{
    /// <summary>One entry in the season picker.</summary>
    public class SeasonDto
    {
        /// <summary>Round-tripped by the client, e.g. "2025-summer".</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Season name in lower case: "spring", "summer", "autumn", "winter".
        /// The client translates this — the picker has to read in the member's
        /// own language, and a server-built label could only ever be in one.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The year the season begins in. Winter runs into the next one, which
        /// the client shows as "2026/27".
        /// </summary>
        public int Year { get; set; }

        /// <summary>English label, used only if a translation is missing.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>True for the season happening right now.</summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>One row of the season leaderboard.</summary>
    public class LeaderboardEntryDto
    {
        public int UserId { get; set; }

        /// <summary>Ready to display: first name, @username, or a placeholder.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Null when the member has no Telegram picture — show initials.</summary>
        public string? PhotoUrl { get; set; }

        /// <summary>Points earned inside the selected season only.</summary>
        public int Points { get; set; }

        /// <summary>1-based, with ties sharing a place.</summary>
        public int Rank { get; set; }

        /// <summary>Lets the client highlight the viewer's own row.</summary>
        public bool IsCurrentUser { get; set; }
    }

    /// <summary>The leaderboard tab's payload: the picker and the selected page.</summary>
    public class LeaderboardDto
    {
        public IReadOnlyList<SeasonDto> Seasons { get; set; } = [];

        public string SelectedSeasonKey { get; set; } = string.Empty;

        public IReadOnlyList<LeaderboardEntryDto> Entries { get; set; } = [];
    }
}
