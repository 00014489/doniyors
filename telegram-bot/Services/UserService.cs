using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using telegram_bot.DAL.Entities;
using telegram_bot.DAL.Repositories.Sessions;
using telegram_bot.DAL.Repositories.Users;
using telegram_bot.Helpers;

namespace telegram_bot.Services
{
    public class UserService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserService> _logger;
        private readonly SessionService _sessionService;

        public UserService(IUserRepository repo, SessionService sessionService, ILogger<UserService> logger)
        {
            _repo = repo;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<User> CreateOrUpdateUserAsync(long tgUserId, string userName, string languageCode)
        {
            var user = await _repo.GetByTgUserIdAsync(tgUserId);

            if (user is not null)
            {
                var isUpdated = false;

                if (user.UserName != userName)
                {
                    _logger.LogInformation(
                        "Username updated. TgUserId={TgUserId}, OldUserName={OldUserName}, NewUserName={NewUserName}",
                        tgUserId,
                        user.UserName,
                        userName);

                    user.UserName = userName;
                    isUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(languageCode) && user.LanguageCode != languageCode)
                {
                    _logger.LogInformation(
                        "Language updated. TgUserId={TgUserId}, OldLanguage={OldLanguage}, NewLanguage={NewLanguage}",
                        tgUserId,
                        string.IsNullOrEmpty(user.LanguageCode) ? "empty" : user.LanguageCode,
                        languageCode);

                    user.LanguageCode = languageCode;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    user.UpdatedAt = DateTimeOffset.UtcNow;
                    await _repo.SaveChangesAsync();
                }

                return user;
            }

            var newUser = new User
            {
                TgUserId = tgUserId,
                UserName = userName,
                QrToken = TokenGenerator.Generate(),
                TypeUserId = 3,
                Points = 0,
                RegisteredAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? string.Empty : languageCode
            };

            
            await _repo.AddAsync(newUser);
            await _repo.SaveChangesAsync();
            _logger.LogInformation(
                "User created successfully. TgUserId={TgUserId}, UserName={UserName}, LanguageCode={LanguageCode}",
                tgUserId,
                userName,
                string.IsNullOrEmpty(newUser.LanguageCode) ? "empty" : newUser.LanguageCode);
            await _sessionService.GetOrCreateSessionAsync(tgUserId);
            return newUser;
        }
        
        public async Task UpdateLanguage(long tgUserId, string languageCode)
        {
            var user = await _repo.GetByTgUserIdAsync(tgUserId);
            if (user is null)
            {
                _logger.LogWarning("User not found. TgUserId={TgUserId}", tgUserId);
                return;
            }

            if (user.LanguageCode == languageCode)
            {
                _logger.LogInformation("Language is already set to the same value. TgUserId={TgUserId}, LanguageCode={LanguageCode}", tgUserId, languageCode);
                return;
            }

            await _repo.UpdateLanguageAsync(tgUserId, languageCode);
            _logger.LogInformation("Language updated successfully. TgUserId={TgUserId}, NewLanguageCode={NewLanguageCode}", tgUserId, languageCode);
        }

        public async Task<bool> IsAdminAsync(long tgUserId)
        {
            return await _repo.GetUserTypeAsync(tgUserId) != 3;
        }

        public async Task<User?> GetUserByTgIdAsync(long tgUserId)
        {
            return await _repo.GetByTgUserIdAsync(tgUserId);
        }
        
    }
}