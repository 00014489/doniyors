using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doniyors.Data.Entities;

namespace telegram_bot.DAL.Repositories.Sessions
{
    public interface ISessionRepo
    {
        Task<UserSession> CreateAsync(long userId, CancellationToken cancellationToken = default);

        Task<SessionStep> GetStepAsync(long userId, CancellationToken cancellationToken = default);

        Task SetStepAsync(long userId, SessionStep step, CancellationToken cancellationToken = default);

        Task<T?> GetDataAsync<T>(long userId, CancellationToken cancellationToken = default);

        Task SetDataAsync<T>(long userId, T data, CancellationToken cancellationToken = default);

        Task UpdateAsync<T>(long userId, SessionStep step, T data, CancellationToken cancellationToken = default);

        Task ClearAsync(long userId, CancellationToken cancellationToken = default);
        Task <UserSession?> GetSessionByIdAsync(long userId, CancellationToken cancellationToken = default);
    }
}