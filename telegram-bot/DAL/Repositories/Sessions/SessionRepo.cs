using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using telegram_bot.DAL.Entities;

namespace telegram_bot.DAL.Repositories.Sessions
{
    public class SessionRepo: ISessionRepo
    {
        private readonly BotDbContext _context;

        public SessionRepo(BotDbContext context)
        {
            _context = context;
        }

        public async Task<UserSession?> GetSessionByIdAsync(long userId)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(x => x.UserId == userId);
        }
        public async Task<UserSession> CreateAsync(long userId)
        {
            var session = new UserSession
            {
                UserId = userId,
                Step = SessionStep.None,
                DataJson = "{}",
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<SessionStep> GetStepAsync(long userId)
        {
            return await _context.UserSessions
                .Where(x => x.UserId == userId)
                .Select(x => x.Step)
                .FirstOrDefaultAsync();
        }

        public async Task SetStepAsync(long userId, SessionStep step)
        {
            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Step, step)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
        }

        public async Task<T?> GetDataAsync<T>(long userId)
        {
            var session = await GetSessionByIdAsync(userId);

            if (string.IsNullOrWhiteSpace(session?.DataJson))
                return default;

            return JsonSerializer.Deserialize<T>(session.DataJson);
        }

        public async Task SetDataAsync<T>(long userId, T data)
        {
            var json = JsonSerializer.Serialize(data);

            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.DataJson, json)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
        }

        public async Task UpdateAsync<T>(long userId, SessionStep step, T data)
        {
            var json = JsonSerializer.Serialize(data);

            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.Step, step)
                    .SetProperty(x => x.DataJson, json)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
        }

        public async Task ClearAsync(long userId)
        {
            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.Step, SessionStep.None)
                    .SetProperty(x => x.DataJson, "{}")
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow));
        }

    }
}