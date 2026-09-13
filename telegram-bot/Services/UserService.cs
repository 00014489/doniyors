using Doniyors.Data;
using Doniyors.Data.Entities;
using telegram_bot.DAL.Repositories.Users;

namespace telegram_bot.Services
{
    public class UserService
    {
        private readonly IUserRepository _repo;
        private readonly SessionService _sessionService;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository repo,
            SessionService sessionService,
            TimeProvider timeProvider,
            ILogger<UserService> logger)
        {
            _repo = repo;
            _sessionService = sessionService;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        /// <summary>
        /// Registers the member on first contact, or brings their stored
        /// details back in line with Telegram. The rule itself lives in
        /// <see cref="UserProvisioning"/>, shared with the API, so both sides
        /// of the product store the same thing for the same person.
        /// </summary>
        public async Task<User> CreateOrUpdateUserAsync(
            TelegramProfile profile,
            CancellationToken cancellationToken = default)
        {
            var now = _timeProvider.GetUtcNow();

            var user = await _repo.GetByTgUserIdAsync(profile.TgUserId, cancellationToken);

            if (user is not null)
            {
                if (UserProvisioning.Apply(user, profile, now))
                {
                    await _repo.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Refreshed profile from Telegram. TgUserId={TgUserId}",
                        profile.TgUserId);
                }

                // A member who talked to the bot before the Mini App existed
                // may have no session row yet.
                await _sessionService.GetOrCreateSessionAsync(profile.TgUserId, cancellationToken);

                return user;
            }

            var created = UserProvisioning.Create(profile, now);

            await _repo.AddAsync(created, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User created. TgUserId={TgUserId}, UserName={UserName}",
                created.TgUserId,
                string.IsNullOrEmpty(created.UserName) ? "<none>" : created.UserName);

            await _sessionService.GetOrCreateSessionAsync(profile.TgUserId, cancellationToken);

            return created;
        }

        /// <summary>
        /// The member's own choice from the language menu. This is the only
        /// thing that may change a language already set — Telegram's client
        /// locale never overrides it.
        /// </summary>
        public async Task UpdateLanguage(
            long tgUserId,
            string languageCode,
            CancellationToken cancellationToken = default)
        {
            // The column is shared with the Mini App and the admin panel, which
            // can only show supported languages.
            if (!SupportedLanguages.IsSupported(languageCode))
            {
                _logger.LogWarning(
                    "Refused an unsupported language. TgUserId={TgUserId}, LanguageCode={LanguageCode}",
                    tgUserId,
                    languageCode);

                return;
            }

            var user = await _repo.GetByTgUserIdAsync(tgUserId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found. TgUserId={TgUserId}", tgUserId);

                return;
            }

            if (user.LanguageCode == languageCode)
                return;

            await _repo.UpdateLanguageAsync(tgUserId, languageCode, cancellationToken);

            _logger.LogInformation(
                "Language updated. TgUserId={TgUserId}, NewLanguageCode={NewLanguageCode}",
                tgUserId,
                languageCode);
        }

        /// <summary>Anything other than the plain member role may scan QR codes.</summary>
        public async Task<bool> IsAdminAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            var roleId = await _repo.GetUserTypeAsync(tgUserId, cancellationToken);

            return roleId != 0 && roleId != UserProvisioning.MemberRoleId;
        }

        public Task<User?> GetUserByTgIdAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return _repo.GetByTgUserIdAsync(tgUserId, cancellationToken);
        }
    }
}
