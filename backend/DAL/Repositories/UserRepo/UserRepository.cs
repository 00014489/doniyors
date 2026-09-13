using Doniyors.Data.Entities;
using backend.DTOs;
using backend.Services.Common;
using Microsoft.EntityFrameworkCore;
using Doniyors.Data;

namespace backend.DAL.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByTelegramIdAsync(
            long telegramId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(u => u.TypeUser)
                .FirstOrDefaultAsync(u => u.TgUserId == telegramId, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public Task<QRCodeDto?> GetQrTokenByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new QRCodeDto
                {
                    QrToken = u.QrToken,
                    Points = u.Points,
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<User?> GetByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public Task<IReadOnlyList<UserTransactionDto>> GetUserTransactionsAsync(
            long tgUserId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
        {
            // `to` is exclusive — see Season.CurrentRange.
            return QueryTransactions(
                _context.Transactions.Where(t =>
                    t.User.TgUserId == tgUserId
                    && t.Status
                    && t.CreatedAt >= from
                    && t.CreatedAt < to),
                cancellationToken);
        }

        public Task<IReadOnlyList<UserTransactionDto>> GetUserAllTransactionsAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return QueryTransactions(
                _context.Transactions.Where(t => t.User.TgUserId == tgUserId && t.Status),
                cancellationToken);
        }

        public Task RecalculatePointsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Shared with the bot, which credits members from scanned QR codes.
            return _context.RecalculatePointsAsync(userId, cancellationToken);
        }

        // ---- Admin panel ----

        public async Task<IReadOnlyList<User>> GetAllWithTypeAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.TypeUser)
                .OrderByDescending(u => u.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public Task<User?> GetWithTypeAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.TypeUser)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<User?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(u => u.TypeUser)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<bool> TelegramIdExistsAsync(
            long telegramId,
            int? excludeUserId = null,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AnyAsync(
                    u => u.TgUserId == telegramId
                        && (excludeUserId == null || u.Id != excludeUserId),
                    cancellationToken);
        }

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
        }

        public Task<bool> RoleExistsAsync(
            int typeUserId,
            CancellationToken cancellationToken = default)
        {
            return _context.TypeUsers.AnyAsync(t => t.Id == typeUserId, cancellationToken);
        }

        public async Task<IReadOnlyList<TypeUser>> GetRolesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.TypeUsers
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Shared projection. The season label is computed after materialising
        /// because it is C# the provider cannot translate.
        /// </summary>
        private static async Task<IReadOnlyList<UserTransactionDto>> QueryTransactions(
            IQueryable<Transaction> query,
            CancellationToken cancellationToken)
        {
            var rows = await query
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new UserTransactionDto
                {
                    TgUserId = t.User.TgUserId,
                    TravelTitle = t.Travel != null ? t.Travel.Title : "Manual adjustment",
                    Points = t.Points,
                    CreatedAt = t.CreatedAt,
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.Season = Season.LabelOf(row.CreatedAt);
            }

            return rows;
        }
    }
}
