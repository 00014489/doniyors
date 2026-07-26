using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.UserService
{
    public interface IUserService
    {
        Task<QRCodeDto?> GetQrCodeAsync(int userId);
        Task<List<UserTransactionDto>> GetUserTransactionsAsync(long tgUserId, int userId);
    }
}