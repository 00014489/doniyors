using backend.DAL.Repositories.UserRepo;
using backend.DTOs;
using backend.DTOs.Member;
using backend.Services.Common;
using backend.Services.TelegramMenuButton;
using Doniyors.Data;

namespace backend.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMenuButton _menuButton;

        public UserService(IUserRepository userRepository, ITelegramMenuButton menuButton)
        {
            _userRepository = userRepository;
            _menuButton = menuButton;
        }

        public Task<QRCodeDto?> GetQrCodeAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Projected in the query: the QR endpoint needs two columns, not a row.
            return _userRepository.GetQrTokenByIdAsync(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<UserTransactionDto>?> GetUserTransactionsAsync(
            long tgUserId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (currentUser is null)
                return null;

            // Your own history in full; someone else's only for the running season.
            if (currentUser.TgUserId == tgUserId)
            {
                return await _userRepository.GetUserAllTransactionsAsync(
                    tgUserId,
                    cancellationToken);
            }

            var season = Season.Of(DateTimeOffset.UtcNow);

            return await _userRepository.GetUserTransactionsAsync(
                tgUserId,
                season.Start,
                season.End,
                cancellationToken);
        }

        public async Task<string?> GetLanguageAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            return user?.LanguageCode;
        }

        public async Task<SaveResult<LanguageDto>> SetLanguageAsync(
            int userId,
            string? languageCode,
            CancellationToken cancellationToken = default)
        {
            // A member picks a language; clearing it back to "not chosen" is not
            // something they do, so the empty string is refused here too.
            if (!SupportedLanguages.IsSupported(languageCode))
            {
                return SaveResult<LanguageDto>.Conflict(
                    ErrorCodes.UnsupportedLanguage,
                    "This language is not supported.");
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
                return SaveResult<LanguageDto>.NotFound("User not found.");

            if (user.LanguageCode != languageCode)
            {
                user.LanguageCode = languageCode!;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                await _userRepository.SaveAsync(cancellationToken);

                await _menuButton.UpdateAsync(user.TgUserId, user.LanguageCode, cancellationToken);
            }

            return SaveResult<LanguageDto>.Success(new LanguageDto { LanguageCode = user.LanguageCode });
        }
    }
}
