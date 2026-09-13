using Doniyors.Data.Entities;
using backend.DTOs;
using backend.DTOs.Member;
using Microsoft.EntityFrameworkCore;
using Doniyors.Data;

namespace backend.DAL.Repositories.MemberRepo
{
    public class MemberRepository : IMemberRepository
    {
        private readonly AppDbContext _context;

        public MemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdventureListItemDto>> GetAdventuresAsync(
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            // Only adventures that have not happened yet, soonest first. The
            // list is something a member browses to decide what to join, so a
            // trip they can no longer attend is noise.
            //
            // Past adventures stay reachable: they appear in the member's own
            // profile history, and their detail page is not date-filtered.
            return await Project(
                    _context.Travels
                        .Where(t => t.Status && t.TravelDate >= now)
                        .OrderBy(t => t.TravelDate),
                    now)
                .ToListAsync(cancellationToken);
        }

        public async Task<AdventureDetailDto?> GetAdventureAsync(
            int id,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            return await _context.Travels
                .AsNoTracking()
                .Where(t => t.Id == id && t.Status)
                .Select(t => new AdventureDetailDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TravelDate = t.TravelDate,
                    Cost = t.Cost,
                    Points = t.Points,
                    IsUpcoming = t.TravelDate >= now,

                    CoverImageId = t.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => (int?)image.Id)
                        .FirstOrDefault(),

                    Images = t.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => new TravelImageDto
                        {
                            Id = image.Id,
                            Title = image.Title,
                            ContentType = image.ContentType,
                            SortOrder = image.SortOrder,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<DateTimeOffset>> GetTransactionMonthsAsync(
            CancellationToken cancellationToken = default)
        {
            // One timestamp per month that has activity. Grouping in the
            // database keeps this O(months) instead of O(transactions), and the
            // caller folds months into seasons.
            return await _context.Transactions
                .AsNoTracking()
                .Where(t => t.Status)
                .Select(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Distinct()
                .Select(x => new DateTimeOffset(x.Year, x.Month, 1, 0, 0, 0, TimeSpan.Zero))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            int limit,
            CancellationToken cancellationToken = default)
        {
            // Every season starts from zero: only transactions stamped inside
            // this window count, so last season's total never carries over.
            // `to` is exclusive — see Season.End.
            //
            // Corrections count too. Summing only positive rows would let a
            // member keep points an administrator has since taken back, so the
            // whole window is summed and the total is filtered afterwards.
            var rows = await _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.Status
                    && t.User.Status
                    && t.CreatedAt >= from
                    && t.CreatedAt < to)
                .GroupBy(t => new
                {
                    t.UserId,
                    t.User.UserName,
                    t.User.FirstName,
                    t.User.PhotoUrl,
                })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.UserName,
                    g.Key.FirstName,
                    g.Key.PhotoUrl,
                    Points = g.Sum(t => t.Points),
                })
                .Where(x => x.Points > 0)
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.UserId)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new LeaderboardEntryDto
                {
                    UserId = row.UserId,
                    DisplayName = DisplayName(row.FirstName, row.UserName),
                    PhotoUrl = row.PhotoUrl,
                    Points = row.Points,
                })
                .ToList();
        }

        public Task<User?> GetProfileUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public Task<int> CountEarningTransactionsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Positive *and* tied to an adventure. Counting every positive
            // transaction would let a manual bonus read as a trip the member
            // never took, and disagree with the history listed beneath it.
            return _context.Transactions
                .AsNoTracking()
                .CountAsync(
                    t => t.UserId == userId
                        && t.Status
                        && t.Points > 0
                        && t.TravelId != null,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<ProfileHistoryItemDto>> GetHistoryAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Adventures only. A manual points adjustment has no date, no
            // picture and no detail page, so it has nothing to render in a
            // list built from adventure cards.
            return await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Status && t.Travel != null)
                .OrderByDescending(t => t.Travel!.TravelDate)
                .Select(t => new ProfileHistoryItemDto
                {
                    TransactionId = t.Id,
                    AdventureId = t.TravelId,
                    Title = t.Travel!.Title,
                    Date = t.Travel.TravelDate,
                    Points = t.Points,

                    CoverImageId = t.Travel.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => (int?)image.Id)
                        .FirstOrDefault(),
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Telegram guarantees a first name but not a @username, so the first
        /// name leads and the username is only a fallback.
        /// </summary>
        public static string DisplayName(string? firstName, string? userName)
        {
            if (!string.IsNullOrWhiteSpace(firstName))
                return firstName;

            if (!string.IsNullOrWhiteSpace(userName))
                return userName;

            return "Member";
        }

        /// <summary>Shared list shape — metadata and a cover id, never bytes.</summary>
        private static IQueryable<AdventureListItemDto> Project(
            IQueryable<Travel> query,
            DateTimeOffset now) =>
            query
                .AsNoTracking()
                .Select(t => new AdventureListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    TravelDate = t.TravelDate,
                    Cost = t.Cost,
                    Points = t.Points,
                    IsUpcoming = t.TravelDate >= now,

                    CoverImageId = t.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => (int?)image.Id)
                        .FirstOrDefault(),
                });
    }
}
