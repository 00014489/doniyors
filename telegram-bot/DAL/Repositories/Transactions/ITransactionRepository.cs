using telegram_bot.Models;

namespace telegram_bot.DAL.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        /// <summary>
        /// Credits the member holding <paramref name="qrToken"/> with
        /// <paramref name="points"/> for an adventure — at most once per member
        /// per adventure — and brings their balance up to date, all in one
        /// database transaction.
        /// </summary>
        Task<ScanAward> AwardAdventureAsync(
            string qrToken,
            int travelId,
            int points,
            DateTimeOffset now,
            CancellationToken cancellationToken = default);
    }
}
