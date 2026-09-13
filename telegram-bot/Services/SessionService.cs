using Doniyors.Data.Entities;
using telegram_bot.DAL.Repositories.Sessions;
using telegram_bot.Models;

namespace telegram_bot.Services
{
    public class SessionService
    {
        private readonly ISessionRepo _sessionRepo;

        public SessionService(ISessionRepo sessionRepo)
        {
            _sessionRepo = sessionRepo;
        }

        public async Task<UserSession> GetOrCreateSessionAsync(
            long userId,
            CancellationToken cancellationToken = default)
        {
            return await _sessionRepo.GetSessionByIdAsync(userId, cancellationToken)
                ?? await _sessionRepo.CreateAsync(userId, cancellationToken);
        }

        /// <summary>
        /// Moves the member into a conversation step.
        /// <para>
        /// The row is created first if it is missing. A member who registered
        /// through the Mini App has a <c>Users</c> row but no session, and the
        /// underlying update touches zero rows in that case — the step would
        /// silently never be set, and their next message would not be read as
        /// the answer the bot is waiting for.
        /// </para>
        /// </summary>
        public async Task SetSessionStepAsync(
            long tgUserId,
            SessionStep step,
            CancellationToken cancellationToken = default)
        {
            await GetOrCreateSessionAsync(tgUserId, cancellationToken);

            await _sessionRepo.SetStepAsync(tgUserId, step, cancellationToken);
        }

        /// <summary>
        /// The current step, or <see cref="SessionStep.None"/> when the member
        /// has no session row — both mean "not in a flow".
        /// </summary>
        public Task<SessionStep> GetSessionStepAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return _sessionRepo.GetStepAsync(tgUserId, cancellationToken);
        }

        /// <summary>
        /// Puts an administrator into the QR scanning step for one adventure.
        /// The step and the adventure are written together, so a photo can
        /// never arrive to a session that has one without the other.
        /// </summary>
        public async Task StartQrScanAsync(
            long tgUserId,
            int travelId,
            CancellationToken cancellationToken = default)
        {
            await GetOrCreateSessionAsync(tgUserId, cancellationToken);

            await _sessionRepo.UpdateAsync(
                tgUserId,
                SessionStep.WaitingForPhoto,
                new QrScanSession(travelId),
                cancellationToken);
        }

        /// <summary>The adventure a scanning session credits, or <c>null</c> if it holds none.</summary>
        public async Task<int?> GetQrScanAdventureIdAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            var data = await _sessionRepo.GetDataAsync<QrScanSession>(tgUserId, cancellationToken);

            return data is { TravelId: > 0 } ? data.TravelId : null;
        }

        /// <summary>Ends whatever flow the member is in and forgets its data.</summary>
        public Task ClearAsync(
            long tgUserId,
            CancellationToken cancellationToken = default)
        {
            return _sessionRepo.ClearAsync(tgUserId, cancellationToken);
        }
    }
}
