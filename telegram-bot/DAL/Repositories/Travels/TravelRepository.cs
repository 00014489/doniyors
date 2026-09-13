using Doniyors.Data;
using Microsoft.EntityFrameworkCore;
using telegram_bot.Models;

namespace telegram_bot.DAL.Repositories.Travels
{
    public class TravelRepository : ITravelRepository
    {
        private readonly AppDbContext _context;

        public TravelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ScanAdventure>> GetScannableAsync(
            DateTimeOffset from,
            int limit,
            CancellationToken cancellationToken = default)
        {
            // Projected: the Travels row is small, but its images are not, and
            // none of them are needed to draw a button.
            return await _context.Travels
                .AsNoTracking()
                .Where(t => t.Status && t.TravelDate >= from)
                .OrderBy(t => t.TravelDate)
                .Take(limit)
                .Select(t => new ScanAdventure(t.Id, t.Title, t.TravelDate, t.Points))
                .ToListAsync(cancellationToken);
        }

        public Task<ScanAdventure?> GetActiveAsync(
            int id,
            DateTimeOffset? from = null,
            CancellationToken cancellationToken = default)
        {
            return _context.Travels
                .AsNoTracking()
                .Where(t => t.Id == id && t.Status && (from == null || t.TravelDate >= from))
                .Select(t => new ScanAdventure(t.Id, t.Title, t.TravelDate, t.Points))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
