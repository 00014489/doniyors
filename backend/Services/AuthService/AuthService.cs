using backend.DAL.Repositories.UserRepo;
using backend.DTOs;
using backend.Services.JwtService;
using backend.Services.TelegramValidator;
using Doniyors.Data;
using Doniyors.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly ITelegramValidator _telegramValidator;
        private readonly IJwt _jwtService;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository repo,
            ITelegramValidator telegramValidator,
            IJwt jwtService,
            TimeProvider timeProvider,
            ILogger<AuthService> logger)
        {
            _repo = repo;
            _telegramValidator = telegramValidator;
            _jwtService = jwtService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(
            string initData,
            CancellationToken cancellationToken = default)
        {
            // Throws UnauthorizedAccessException on a bad signature; the global
            // handler maps that to 401.
            var tgUser = _telegramValidator.Validate(initData);

            var profile = ToProfile(tgUser);

            var user = await _repo.GetByTelegramIdAsync(profile.TgUserId, cancellationToken);

            if (user is null)
            {
                user = await CreateUserAsync(profile, cancellationToken);
            }
            else if (UserProvisioning.Apply(user, profile, _timeProvider.GetUtcNow()))
            {
                // Telegram is the source of truth for name and picture, and both
                // change. The Mini App sees the current values on every sign-in,
                // so this is where they are kept fresh.
                await _repo.SaveAsync(cancellationToken);
            }

            if (!user.Status)
            {
                _logger.LogWarning(
                    "Disabled account attempted to sign in. TgUserId={TgUserId}",
                    profile.TgUserId);

                throw new UnauthorizedAccessException("This account has been disabled.");
            }

            return new LoginResponse
            {
                Token = _jwtService.Create(user),
                LanguageCode = user.LanguageCode,
            };
        }

        /// <summary>The Mini App is the only source that carries a picture.</summary>
        private static TelegramProfile ToProfile(TelegramUser tgUser) =>
            new(
                TgUserId: tgUser.Id,
                FirstName: tgUser.FirstName,
                UserName: tgUser.UserName,
                LanguageCode: tgUser.LanguageCode,
                PhotoUrl: tgUser.PhotoUrl);

        private async Task<User> CreateUserAsync(
            TelegramProfile profile,
            CancellationToken cancellationToken)
        {
            var user = UserProvisioning.Create(profile, _timeProvider.GetUtcNow());

            try
            {
                await _repo.AddAsync(user, cancellationToken);
                await _repo.SaveAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // The bot creates users too, and a Mini App can be opened twice
                // at once. TgUserId is unique, so the loser of that race just
                // reads the row the winner inserted.
                var existing = await _repo.GetByTelegramIdAsync(profile.TgUserId, cancellationToken);

                if (existing is null)
                    throw;

                _logger.LogInformation(
                    "Concurrent first sign-in resolved to the existing row. TgUserId={TgUserId}",
                    profile.TgUserId);

                return existing;
            }

            _logger.LogInformation("Created new user. TgUserId={TgUserId}", user.TgUserId);

            // Re-read so the TypeUser navigation is loaded — the token needs the role name.
            return await _repo.GetByTelegramIdAsync(user.TgUserId, cancellationToken) ?? user;
        }
    }
}
