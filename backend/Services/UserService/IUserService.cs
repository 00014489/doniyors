using backend.DTOs;
using backend.DTOs.Member;
using backend.Services.Common;

namespace backend.Services.UserService
{
    public interface IUserService
    {
        Task<QRCodeDto?> GetQrCodeAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns <c>null</c> when the caller's own account cannot be found —
        /// the controller turns that into a 401.
        /// </summary>
        Task<IReadOnlyList<UserTransactionDto>?> GetUserTransactionsAsync(
            long tgUserId,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>The stored language code, or <c>null</c> when the account no longer exists.</summary>
        Task<string?> GetLanguageAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>Stores the member's own choice; only supported codes are accepted.</summary>
        Task<SaveResult<LanguageDto>> SetLanguageAsync(
            int userId,
            string? languageCode,
            CancellationToken cancellationToken = default);
    }
}
