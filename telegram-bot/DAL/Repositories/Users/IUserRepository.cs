using Doniyors.Data.Entities;

namespace telegram_bot.DAL.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByTgUserIdAsync(
            long tgUserId,
            CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task UpdateLanguageAsync(
            long tgUserId,
            string languageCode,
            CancellationToken cancellationToken = default);

        Task<int> GetUserTypeAsync(
            long tgUserId,
            CancellationToken cancellationToken = default);
    }
}
