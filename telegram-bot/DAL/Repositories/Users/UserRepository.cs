using Microsoft.EntityFrameworkCore;
using Doniyors.Data.Entities;
using Doniyors.Data;

namespace telegram_bot.DAL.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByTgUserIdAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .FirstOrDefaultAsync(x => x.TgUserId == tgUserId, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateLanguageAsync(
            long tgUserId,
            string languageCode,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Where(u => u.TgUserId == tgUserId)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(u => u.LanguageCode, languageCode)
                        .SetProperty(u => u.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        public Task<int> GetUserTypeAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Where(u => u.TgUserId == tgUserId)
                .Select(u => u.TypeUserId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
