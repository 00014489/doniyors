using Doniyors.Data.Entities;
using backend.DTOs;

namespace backend.DAL.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<User?> GetByTelegramIdAsync(
            long tgId,
            CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            User user,
            CancellationToken cancellationToken = default);

        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<QRCodeDto?> GetQrTokenByIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserTransactionDto>> GetUserTransactionsAsync(
            long tgUserId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserTransactionDto>> GetUserAllTransactionsAsync(
            long tgUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Rewrites <c>Points</c> from the sum of the user's active
        /// transactions, in one statement. This is the only writer of the
        /// column, so the cached balance cannot drift from its history.
        /// </summary>
        Task RecalculatePointsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        // ---- Admin panel ----

        Task<IReadOnlyList<User>> GetAllWithTypeAsync(
            CancellationToken cancellationToken = default);

        Task<User?> GetWithTypeAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>Tracked lookup, used when the user is about to be modified.</summary>
        Task<User?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>Telegram ids are unique; <paramref name="excludeUserId"/> skips the row being edited.</summary>
        Task<bool> TelegramIdExistsAsync(
            long telegramId,
            int? excludeUserId = null,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> RoleExistsAsync(
            int typeUserId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TypeUser>> GetRolesAsync(
            CancellationToken cancellationToken = default);
    }
}
