using Doniyors.Data.Entities;
using backend.DTOs.Member;

namespace backend.Services.MemberService
{
    public interface IMemberService
    {
        Task<IReadOnlyList<AdventureListItemDto>> GetAdventuresAsync(
            CancellationToken cancellationToken = default);

        Task<AdventureDetailDto?> GetAdventureAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// The leaderboard tab. <paramref name="seasonKey"/> is null on first
        /// load, which selects the current season.
        /// </summary>
        Task<LeaderboardDto> GetLeaderboardAsync(
            string? seasonKey,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>Returns <c>null</c> when the account no longer exists.</summary>
        Task<ProfileDto?> GetProfileAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<TravelImage?> GetImageAsync(
            int imageId,
            CancellationToken cancellationToken = default);
    }
}
