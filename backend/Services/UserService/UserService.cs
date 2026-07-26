using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Repositories.UserRepo;
using backend.DTOs;

namespace backend.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<QRCodeDto?> GetQrCodeAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
            {
                return null;
            }

            return new QRCodeDto
            {
                QrToken = user.QrToken,
                Points = user.Points
            };
        }
        public async Task<List<UserTransactionDto>> GetUserTransactionsAsync(
            long tgUserId, int userId)
        {
            var currentUser = await _userRepository.GetByIdAsync(userId);

            if (currentUser is null)
                throw new Exception("User not found.");

            if (currentUser.TgUserId == tgUserId)
            {
                return await _userRepository.GetUserAllTransactionsAsync(tgUserId);
            }

            var from = GetSeasonStart();
            var to = GetSeasonEnd();

            return await _userRepository.GetUserTransactionsAsync(
                tgUserId,
                from,
                to);
        }
        private static DateTime GetSeasonStart()
        {
            var now = DateTime.UtcNow;
            var year = now.Year;

            return now.Month switch
            {
                >= 3 and <= 5 => new DateTime(year, 3, 1),   // Spring
                >= 6 and <= 8 => new DateTime(year, 6, 1),   // Summer
                >= 9 and <= 11 => new DateTime(year, 9, 1),  // Autumn
                _ => new DateTime(now.Month == 12 ? year : year - 1, 12, 1) // Winter
            };
        }

        private static DateTime GetSeasonEnd()
        {
            var now = DateTime.UtcNow;
            var year = now.Year;

            return now.Month switch
            {
                >= 3 and <= 5 => new DateTime(year, 5, 31, 23, 59, 59),
                >= 6 and <= 8 => new DateTime(year, 8, 31, 23, 59, 59),
                >= 9 and <= 11 => new DateTime(year, 11, 30, 23, 59, 59),
                _ => new DateTime(now.Month == 12 ? year + 1 : year, 2,
                        DateTime.IsLeapYear(now.Month == 12 ? year + 1 : year) ? 29 : 28,
                        23, 59, 59)
            };
        }
        
        
    }
}