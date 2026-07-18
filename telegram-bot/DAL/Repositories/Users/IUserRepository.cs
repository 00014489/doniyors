using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using telegram_bot.DAL.Entities;

namespace telegram_bot.DAL.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByTgUserIdAsync(long tgUserId);

        Task AddAsync(User user);

        void UpdateAsync(User user);

        Task SaveChangesAsync();
        Task UpdateLanguageAsync(long tgUserId, string languageCode);
        Task<int> GetUserTypeAsync(long tgUserId);
    }
}