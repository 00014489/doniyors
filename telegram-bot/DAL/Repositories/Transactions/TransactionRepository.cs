using Doniyors.Data;
using Doniyors.Data.Entities;
using Microsoft.EntityFrameworkCore;
using telegram_bot.Models;

namespace telegram_bot.DAL.Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScanAward> AwardAdventureAsync(
            string qrToken,
            int travelId,
            int points,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            // Leaving without committing — every early return below — rolls
            // back and releases the row lock.
            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            // FOR UPDATE holds the member's row until commit. Without it, two
            // administrators scanning the same person at once — or Telegram
            // redelivering a photo whose first delivery is still being
            // processed — would both pass the duplicate check below and credit
            // the adventure twice.
            //
            // Materialised directly: composing a LINQ operator onto FromSql
            // would wrap the locking query in a subquery.
            var matches = await _context.Users
                .FromSql($"""SELECT * FROM "Users" WHERE "QrToken" = {qrToken} FOR UPDATE""")
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // QrToken is unique, so there is one row or none.
            var user = matches.SingleOrDefault();

            if (user is null)
                return new ScanAward(ScanAwardStatus.UnknownCode);

            if (!user.Status)
            {
                return new ScanAward(
                    ScanAwardStatus.UserDisabled,
                    user.Id,
                    user.FirstName,
                    user.UserName,
                    user.Points);
            }

            // One credit per member per adventure. A voided transaction does not
            // count, so a credit an administrator reversed can be scanned again.
            var alreadyAwarded = await _context.Transactions
                .AnyAsync(
                    t => t.UserId == user.Id && t.TravelId == travelId && t.Status,
                    cancellationToken);

            if (alreadyAwarded)
            {
                return new ScanAward(
                    ScanAwardStatus.AlreadyAwarded,
                    user.Id,
                    user.FirstName,
                    user.UserName,
                    user.Points);
            }

            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                TravelId = travelId,
                Points = points,
                Status = true,
                CreatedAt = now,
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _context.RecalculatePointsAsync(user.Id, cancellationToken);

            var balance = await _context.Users
                .Where(u => u.Id == user.Id)
                .Select(u => u.Points)
                .SingleAsync(cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);

            return new ScanAward(
                ScanAwardStatus.Awarded,
                user.Id,
                user.FirstName,
                user.UserName,
                balance);
        }
    }
}
