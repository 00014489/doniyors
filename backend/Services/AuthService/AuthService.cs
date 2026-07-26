using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Entities;
using backend.DAL.Repositories.UserRepo;
using backend.DTOs;
using backend.Helpers;
using backend.Services.JwtService;
using backend.Services.TelegramValidator;

namespace backend.Services.AuthService
{
    public class AuthService: IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly ITelegramValidator _telegramValidator;
        private readonly IJwt _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository repo,
            ITelegramValidator telegramValidator,
            IJwt jwtService,
            ILogger<AuthService> logger)
        {
            _repo = repo;
            _telegramValidator = telegramValidator;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(string initData)
        {
            // Validate Telegram initData
            var tgUser = _telegramValidator.Validate(initData);

            // Reject Telegram bots
            if (tgUser.IsBot)
            {
                _logger.LogWarning(
                    "Bot attempted to access Mini App. TgUserId={TgUserId}",
                    tgUser.Id);

                throw new UnauthorizedAccessException("Bots are not allowed.");
            }

            // Find existing user
            var user = await _repo.GetByTelegramIdAsync(tgUser.Id);

            // Create user if it doesn't exist
            if (user is null)
            {
                user = await CreateUserAsync(tgUser);
            }

            return new LoginResponse
            {
                Token = _jwtService.Create(user),
                LanguageCode = user.LanguageCode
            };
        }

        private async Task<User> CreateUserAsync(TelegramUser tgUser)
        {
            var user = new User
            {
                TgUserId = tgUser.Id,
                UserName = tgUser.UserName ?? string.Empty,
                LanguageCode = tgUser.LanguageCode ?? string.Empty,
                QrToken = TokenGenerator.Generate(),
                TypeUserId = 3, // Ordinary user
                Points = 0,
                RegisteredAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(user);
            await _repo.SaveAsync();

            _logger.LogInformation(
                "Created new user. TgUserId={TgUserId}",
                user.TgUserId);

            return user;
        }
    }
}