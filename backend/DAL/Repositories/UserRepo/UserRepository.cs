using backend.DAL.Entities;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.DAL.Repositories.UserRepo
{
    public class UserRepository: IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _context.Users
                .Include(u => u.TypeUser)
                .FirstOrDefaultAsync(u => u.TgUserId == telegramId);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<QRCodeDto?> GetQrTokenByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new QRCodeDto
                {
                    QrToken = u.QrToken,
                    Points = u.Points
                })
                .FirstOrDefaultAsync();
        }

        public Task<User?> GetByIdAsync(int userId)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<UserTransactionDto>> GetUserTransactionsAsync(
            long tgUserId,
            DateTimeOffset from,
            DateTimeOffset to)
        {
            var transactions = await _context.Transactions
                .Where(t =>
                    t.User.TgUserId == tgUserId &&
                    t.CreatedAt >= from &&
                    t.CreatedAt <= to)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new UserTransactionDto
                {
                    TgUserId = t.User.TgUserId,
                    TravelTitle = t.Travel.Title,
                    Points = t.Travel.Points,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.Season = GetSeason(transaction.CreatedAt);
            }

            return transactions;
        }
        public async Task<List<UserTransactionDto>> GetUserAllTransactionsAsync(long tgUserId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.User.TgUserId == tgUserId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new UserTransactionDto
                {
                    TgUserId = t.User.TgUserId,
                    TravelTitle = t.Travel.Title,
                    Points = t.Travel.Points,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.Season = GetSeason(transaction.CreatedAt);
            }

            return transactions;
        }

        private static string GetSeason(DateTimeOffset date)
        {
            return date.Month switch
            {
                >= 3 and <= 5 =>
                    $"Spring (03.{date.Year} - 05.{date.Year})",

                >= 6 and <= 8 =>
                    $"Summer (06.{date.Year} - 08.{date.Year})",

                >= 9 and <= 11 =>
                    $"Autumn (09.{date.Year} - 11.{date.Year})",

                _ =>
                    date.Month == 12
                        ? $"Winter (12.{date.Year} - 02.{date.Year + 1})"
                        : $"Winter (12.{date.Year - 1} - 02.{date.Year})"
            };
        }
    }
}