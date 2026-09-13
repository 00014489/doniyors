using telegram_bot.Models;

namespace telegram_bot.DAL.Repositories.Travels
{
    public interface ITravelRepository
    {
        /// <summary>Active adventures dated at or after <paramref name="from"/>, soonest first.</summary>
        Task<IReadOnlyList<ScanAdventure>> GetScannableAsync(
            DateTimeOffset from,
            int limit,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// An active adventure, or <c>null</c>. When <paramref name="from"/> is
        /// given, the adventure must also be dated at or after it.
        /// </summary>
        Task<ScanAdventure?> GetActiveAsync(
            int id,
            DateTimeOffset? from = null,
            CancellationToken cancellationToken = default);
    }
}
