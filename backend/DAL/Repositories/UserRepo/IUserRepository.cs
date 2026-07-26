using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Entities;
using backend.DTOs;

namespace backend.DAL.Repositories.UserRepo
{
    public interface IUserRepository
    {
        Task<User?> GetByTelegramIdAsync(long tgId);
        Task<User?> GetByIdAsync(int userId);

        Task AddAsync(User user);

        Task SaveAsync();
        Task<QRCodeDto?> GetQrTokenByIdAsync(int userId);
        Task<List<UserTransactionDto>> GetUserTransactionsAsync(long tgUserId, DateTimeOffset from, DateTimeOffset to);
        Task<List<UserTransactionDto>> GetUserAllTransactionsAsync(long tgUserId);
    }
}