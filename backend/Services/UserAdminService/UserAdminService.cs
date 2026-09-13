using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Doniyors.Data.Entities;
using backend.DAL.Repositories.UserRepo;
using backend.DTOs;
using Doniyors.Data;
using backend.Services.Common;
using backend.Services.TelegramMenuButton;

namespace backend.Services.UserAdminService
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMenuButton _menuButton;

        public UserAdminService(IUserRepository userRepository, ITelegramMenuButton menuButton)
        {
            _userRepository = userRepository;
            _menuButton = menuButton;
        }

        public async Task<IReadOnlyList<UserAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllWithTypeAsync(cancellationToken);

            return users
                .Select(Map)
                .ToList();
        }

        public async Task<UserAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetWithTypeAsync(id, cancellationToken);

            return user is null
                ? null
                : Map(user);
        }

        public async Task<IReadOnlyList<UserRoleDto>> GetRolesAsync(
            CancellationToken cancellationToken = default)
        {
            var roles = await _userRepository.GetRolesAsync(cancellationToken);

            return roles
                .Select(role => new UserRoleDto
                {
                    Id = role.Id,
                    Name = role.Name
                })
                .ToList();
        }

        public async Task<SaveResult<UserAdminDto>> CreateAsync(
            UserSaveDto request,
            CancellationToken cancellationToken = default)
        {
            if (!TryReadLanguage(request.LanguageCode, out var languageCode))
                return UnsupportedLanguage();

            if (!await _userRepository.RoleExistsAsync(request.TypeUserId, cancellationToken))
                return RoleNotFound();

            if (await _userRepository.TelegramIdExistsAsync(
                    request.TgUserId,
                    null,
                    cancellationToken))
            {
                return TelegramIdTaken();
            }

            var user = new User
            {
                UserName = request.UserName.Trim(),
                TgUserId = request.TgUserId,
                LanguageCode = languageCode,
                TypeUserId = request.TypeUserId,
                // Derived from transactions; a new user has none yet.
                Points = 0,
                Status = request.Status,
                // The QR token identifies the user at scan time — always server generated.
                QrToken = TokenGenerator.Generate(),
                RegisteredAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveAsync(cancellationToken);

            // Re-read so the role name is populated for the response.
            var created = await _userRepository.GetWithTypeAsync(user.Id, cancellationToken);

            return SaveResult<UserAdminDto>.Success(created is null ? Map(user) : Map(created));
        }

        public async Task<SaveResult<UserAdminDto>> UpdateAsync(
            int id,
            UserSaveDto request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetForUpdateAsync(id, cancellationToken);

            if (user is null)
                return SaveResult<UserAdminDto>.NotFound("User not found.");

            if (!TryReadLanguage(request.LanguageCode, out var languageCode))
                return UnsupportedLanguage();

            if (!await _userRepository.RoleExistsAsync(request.TypeUserId, cancellationToken))
                return RoleNotFound();

            if (await _userRepository.TelegramIdExistsAsync(
                    request.TgUserId,
                    id,
                    cancellationToken))
            {
                return TelegramIdTaken();
            }

            var buttonNeedsRelabel =
                user.LanguageCode != languageCode || user.TgUserId != request.TgUserId;

            user.UserName = request.UserName.Trim();
            user.TgUserId = request.TgUserId;
            user.LanguageCode = languageCode;
            user.TypeUserId = request.TypeUserId;
            // Points is intentionally not assigned — TransactionService owns it.
            user.Status = request.Status;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _userRepository.SaveAsync(cancellationToken);

            // The bot and the Mini App already read the new value; the menu
            // button is the one label Telegram holds on to.
            if (buttonNeedsRelabel)
                await _menuButton.UpdateAsync(user.TgUserId, user.LanguageCode, cancellationToken);

            var updated = await _userRepository.GetWithTypeAsync(id, cancellationToken);

            return SaveResult<UserAdminDto>.Success(updated is null ? Map(user) : Map(updated));
        }

        public async Task<UserAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetForUpdateAsync(id, cancellationToken);

            if (user is null)
                return null;

            // Soft delete — the row is preserved, only the flag changes.
            if (user.Status)
            {
                user.Status = false;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                await _userRepository.SaveAsync(cancellationToken);
            }

            return Map(user);
        }

        /// <summary>
        /// An administrator may set a supported language or leave it empty
        /// ("not chosen yet" — the bot then asks the member). Nothing else, so the
        /// column only ever holds a language every part of the product can show.
        /// </summary>
        private static bool TryReadLanguage(string? value, out string languageCode)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                languageCode = string.Empty;

                return true;
            }

            languageCode = SupportedLanguages.Normalize(value) ?? string.Empty;

            return languageCode.Length > 0;
        }

        private static SaveResult<UserAdminDto> UnsupportedLanguage() =>
            SaveResult<UserAdminDto>.Conflict(
                ErrorCodes.UnsupportedLanguage,
                "This language is not supported.");

        private static SaveResult<UserAdminDto> RoleNotFound() =>
            SaveResult<UserAdminDto>.Conflict(
                ErrorCodes.RoleNotFound,
                "The selected account type does not exist.");

        private static SaveResult<UserAdminDto> TelegramIdTaken() =>
            SaveResult<UserAdminDto>.Conflict(
                ErrorCodes.TelegramIdTaken,
                "Another user is already registered with this Telegram ID.");

        private static UserAdminDto Map(User user) =>
            new UserAdminDto
            {
                Id = user.Id,
                TgUserId = user.TgUserId,
                UserName = user.UserName,
                LanguageCode = user.LanguageCode,
                Points = user.Points,
                TypeUserId = user.TypeUserId,
                TypeUserName = user.TypeUser?.Name ?? string.Empty,
                RegisteredAt = user.RegisteredAt,
                UpdatedAt = user.UpdatedAt,
                Status = user.Status
            };
    }
}
