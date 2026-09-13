using Doniyors.Data.Entities;
using backend.DTOs.Member;

namespace backend.DAL.Repositories.MemberRepo
{
    /// <summary>
    /// Read model behind the Mini App's member-facing screens. Everything here
    /// is projected in the database — these endpoints are on the hot path and
    /// must never pull image bytes.
    /// </summary>
    public interface IMemberRepository
    {
        /// <summary>
        /// Active adventures that have not happened yet, soonest first.
        /// Past ones are excluded — see the implementation for why.
        /// </summary>
        Task<IReadOnlyList<AdventureListItemDto>> GetAdventuresAsync(
            DateTimeOffset now,
            CancellationToken cancellationToken = default);

        Task<AdventureDetailDto?> GetAdventureAsync(
            int id,
            DateTimeOffset now,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Every season that has at least one active transaction, newest first.
        /// </summary>
        Task<IReadOnlyList<DateTimeOffset>> GetTransactionMonthsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Members ranked by points earned strictly inside the given window.
        /// Only positive earnings count towards a ranking.
        /// </summary>
        Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            int limit,
            CancellationToken cancellationToken = default);

        Task<User?> GetProfileUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>Number of active transactions that added points.</summary>
        Task<int> CountEarningTransactionsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>The member's crediting history, most recent first.</summary>
        Task<IReadOnlyList<ProfileHistoryItemDto>> GetHistoryAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
