using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using telegram_bot.DAL.Entities;

namespace telegram_bot.DAL.Repositories.Users
{
    public class UserRepository: IUserRepository
    {
        private readonly BotDbContext _context;

        public UserRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByTgUserIdAsync(long tgUserId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.TgUserId == tgUserId);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void UpdateAsync(User user)
        {
            _context.Users.Update(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLanguageAsync(long tgUserId, string languageCode)
        {
            await _context.Users
                .Where(u => u.TgUserId == tgUserId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.LanguageCode, languageCode)
                    .SetProperty(u => u.UpdatedAt, DateTimeOffset.UtcNow));
        }

        public async Task<int> GetUserTypeAsync(long tgUserId)
        {
            return await _context.Users
                .Where(u => u.TgUserId == tgUserId)
                .Select(u => u.TypeUserId)
                .FirstOrDefaultAsync();
        }
    }
}