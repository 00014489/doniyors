using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Doniyors.Data.Entities;
using Doniyors.Data;

namespace telegram_bot.DAL.Repositories.Sessions
{
    public class SessionRepo: ISessionRepo
    {
        private readonly AppDbContext _context;

        public SessionRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSession?> GetSessionByIdAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }
        public async Task<UserSession> CreateAsync(long userId, CancellationToken cancellationToken = default)
        {
            var session = new UserSession
            {
                UserId = userId,
                Step = SessionStep.None,
                DataJson = "{}",
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            return session;
        }

        public async Task<SessionStep> GetStepAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions
                .Where(x => x.UserId == userId)
                .Select(x => x.Step)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task SetStepAsync(long userId, SessionStep step, CancellationToken cancellationToken = default)
        {
            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Step, step)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        public async Task<T?> GetDataAsync<T>(long userId, CancellationToken cancellationToken = default)
        {
            var session = await GetSessionByIdAsync(userId, cancellationToken);

            if (string.IsNullOrWhiteSpace(session?.DataJson))
                return default;

            return JsonSerializer.Deserialize<T>(session.DataJson);
        }

        public async Task SetDataAsync<T>(long userId, T data, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data);

            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.DataJson, json)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        public async Task UpdateAsync<T>(long userId, SessionStep step, T data, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(data);

            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.Step, step)
                    .SetProperty(x => x.DataJson, json)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        public async Task ClearAsync(long userId, CancellationToken cancellationToken = default)
        {
            await _context.UserSessions
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.Step, SessionStep.None)
                    .SetProperty(x => x.DataJson, "{}")
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

    }
}