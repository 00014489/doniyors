using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using telegram_bot.DAL.Entities;

namespace telegram_bot.DAL.Repositories.Sessions
{
    public interface ISessionRepo
    {
        Task<UserSession> CreateAsync(long userId);

        Task<SessionStep> GetStepAsync(long userId);

        Task SetStepAsync(long userId, SessionStep step);

        Task<T?> GetDataAsync<T>(long userId);

        Task SetDataAsync<T>(long userId, T data);

        Task UpdateAsync<T>(long userId, SessionStep step, T data);

        Task ClearAsync(long userId);
        Task <UserSession?> GetSessionByIdAsync(long userId);
    }
}