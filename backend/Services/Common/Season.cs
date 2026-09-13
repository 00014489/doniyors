using System.Globalization;

namespace backend.Services.Common
{
    public enum SeasonName
    {
        Winter,
        Spring,
        Summer,
        Autumn,
    }

    /// <summary>
    /// A meteorological season: a three-month window used to slice the
    /// leaderboard. Winter straddles a year boundary, so <see cref="Year"/> is
    /// the year its December falls in — winter 2024 runs Dec 2024 to Feb 2025.
    /// </summary>
    public readonly record struct Season(int Year, SeasonName Name)
    {
        /// <summary>Stable identifier the client round-trips, e.g. "2025-summer".</summary>
        public string Key => $"{Year}-{Name.ToString().ToLowerInvariant()}";

        /// <summary>First moment of the season, inclusive.</summary>
        public DateTimeOffset Start =>
            new(Year, StartMonth, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// First moment of the next season, exclusive. Half-open so nothing can
        /// fall through the gap between "23:59:59" and midnight.
        /// </summary>
        public DateTimeOffset End => Start.AddMonths(3);

        /// <summary>
        /// Shown in the season picker: "Summer 2025", and "Winter 2024/25" for
        /// the one that spans two years.
        /// </summary>
        public string Label =>
            Name == SeasonName.Winter
                ? $"Winter {Year}/{(Year + 1) % 100:D2}"
                : $"{Name} {Year}";

        private int StartMonth =>
            Name switch
            {
                SeasonName.Spring => 3,
                SeasonName.Summer => 6,
                SeasonName.Autumn => 9,
                _ => 12,
            };

        /// <summary>The season a moment falls in.</summary>
        public static Season Of(DateTimeOffset moment)
        {
            var utc = moment.ToUniversalTime();

            return utc.Month switch
            {
                >= 3 and <= 5 => new Season(utc.Year, SeasonName.Spring),
                >= 6 and <= 8 => new Season(utc.Year, SeasonName.Summer),
                >= 9 and <= 11 => new Season(utc.Year, SeasonName.Autumn),

                // January and February belong to the winter that began in December.
                12 => new Season(utc.Year, SeasonName.Winter),
                _ => new Season(utc.Year - 1, SeasonName.Winter),
            };
        }

        /// <summary>The season immediately before this one.</summary>
        public Season Previous() => Of(Start.AddDays(-1));

        public static bool TryParse(string? key, out Season season)
        {
            season = default;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            var parts = key.Split('-', 2);

            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var year))
                return false;

            if (!Enum.TryParse<SeasonName>(parts[1], ignoreCase: true, out var name))
                return false;

            // Reject "2025-Summer " style input that Enum.TryParse would accept
            // as a number, and years outside anything this app could hold.
            if (year is < 2000 or > 2200 || !Enum.IsDefined(name))
                return false;

            season = new Season(year, name);

            return true;
        }

        /// <summary>Label for the season a timestamp falls in.</summary>
        public static string LabelOf(DateTimeOffset moment) => Of(moment).Label;
    }
}
