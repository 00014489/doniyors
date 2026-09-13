using Doniyors.Data.Entities;
using backend.DAL.Repositories.MemberRepo;
using backend.DAL.Repositories.TravelRepo;
using backend.DTOs.Member;
using backend.Services.Common;

namespace backend.Services.MemberService
{
    public class MemberService : IMemberService
    {
        /// <summary>Enough to scroll; the Mini App is not a reporting tool.</summary>
        private const int LeaderboardSize = 100;

        private readonly IMemberRepository _memberRepository;
        private readonly ITravelRepository _travelRepository;
        private readonly TimeProvider _timeProvider;

        public MemberService(
            IMemberRepository memberRepository,
            ITravelRepository travelRepository,
            TimeProvider timeProvider)
        {
            _memberRepository = memberRepository;
            _travelRepository = travelRepository;
            _timeProvider = timeProvider;
        }

        public Task<IReadOnlyList<AdventureListItemDto>> GetAdventuresAsync(
            CancellationToken cancellationToken = default)
        {
            return _memberRepository.GetAdventuresAsync(Now, cancellationToken);
        }

        public Task<AdventureDetailDto?> GetAdventureAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _memberRepository.GetAdventureAsync(id, Now, cancellationToken);
        }

        public Task<TravelImage?> GetImageAsync(
            int imageId,
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetImageAsync(imageId, cancellationToken);
        }

        public async Task<LeaderboardDto> GetLeaderboardAsync(
            string? seasonKey,
            int currentUserId,
            CancellationToken cancellationToken = default)
        {
            var current = Season.Of(Now);

            var seasons = await BuildSeasonListAsync(current, cancellationToken);

            // An unknown or malformed key falls back to the current season
            // rather than erroring — a stale bookmark should still open.
            var selected = Season.TryParse(seasonKey, out var parsed)
                && seasons.Any(s => s.Key == parsed.Key)
                    ? parsed
                    : current;

            var entries = await _memberRepository.GetLeaderboardAsync(
                selected.Start,
                selected.End,
                LeaderboardSize,
                cancellationToken);

            return new LeaderboardDto
            {
                Seasons = seasons.Select(s => new SeasonDto
                {
                    Key = s.Key,
                    Name = s.Name.ToString().ToLowerInvariant(),
                    Year = s.Year,
                    Label = s.Label,
                    IsCurrent = s == current,
                }).ToList(),

                SelectedSeasonKey = selected.Key,
                Entries = Rank(entries, currentUserId),
            };
        }

        public async Task<ProfileDto?> GetProfileAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _memberRepository.GetProfileUserAsync(userId, cancellationToken);

            if (user is null)
                return null;

            var travels = await _memberRepository.CountEarningTransactionsAsync(
                userId,
                cancellationToken);

            var history = await _memberRepository.GetHistoryAsync(userId, cancellationToken);

            return new ProfileDto
            {
                TgUserId = user.TgUserId,
                DisplayName = MemberRepository.DisplayName(user.FirstName, user.UserName),
                UserName = user.UserName,
                PhotoUrl = user.PhotoUrl,
                Points = user.Points,
                TravelsCount = travels,
                RegisteredAt = user.RegisteredAt,
                History = history,
            };
        }

        private DateTimeOffset Now => _timeProvider.GetUtcNow();

        /// <summary>
        /// Seasons that actually have activity, newest first, with the current
        /// one always present so a quiet season still opens.
        /// </summary>
        private async Task<IReadOnlyList<Season>> BuildSeasonListAsync(
            Season current,
            CancellationToken cancellationToken)
        {
            var months = await _memberRepository.GetTransactionMonthsAsync(cancellationToken);

            var seasons = months
                .Select(Season.Of)
                .Append(current)
                .Distinct()
                .OrderByDescending(s => s.Start)
                .ToList();

            return seasons;
        }

        /// <summary>
        /// Places are assigned after the fact so equal scores share a rank and
        /// the next place skips accordingly — 1, 2, 2, 4.
        /// </summary>
        private static List<LeaderboardEntryDto> Rank(
            IReadOnlyList<LeaderboardEntryDto> entries,
            int currentUserId)
        {
            var ranked = new List<LeaderboardEntryDto>(entries.Count);

            var place = 0;
            var previousPoints = int.MinValue;

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];

                if (entry.Points != previousPoints)
                {
                    place = index + 1;
                    previousPoints = entry.Points;
                }

                entry.Rank = place;
                entry.IsCurrentUser = entry.UserId == currentUserId;

                ranked.Add(entry);
            }

            return ranked;
        }
    }
}
