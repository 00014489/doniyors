using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using telegram_bot.DAL.Entities;
using telegram_bot.DAL.Repositories.Sessions;

namespace telegram_bot.Services
{
    public class SessionService
    {
        private readonly ISessionRepo _sessionRepo;
        private readonly ILogger<SessionService> _logger;

        public SessionService(ISessionRepo sessionRepo, ILogger<SessionService> logger)
        {
            _sessionRepo = sessionRepo;
            _logger = logger;
        }
        public async Task<UserSession> GetOrCreateSessionAsync(long userId)
        {
            var session = await _sessionRepo.GetSessionByIdAsync(userId);

            if (session == null)
            {
                session = await _sessionRepo.CreateAsync(userId);
            }

            return session;
        }
        public async Task SetSessionStepAsync(long tgUserId, SessionStep step)
        {
            await _sessionRepo.SetStepAsync(tgUserId, step);
        }

        public async Task<SessionStep?> GetSessionStepAsync(long tgUserId)
        {
            return await _sessionRepo.GetStepAsync(tgUserId);
        }

    }
}