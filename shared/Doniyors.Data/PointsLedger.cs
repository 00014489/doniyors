using Microsoft.EntityFrameworkCore;

namespace Doniyors.Data
{
    /// <summary>
    /// The single rule for a member's balance: <c>Users.Points</c> is the sum of
    /// their active transactions.
    /// <para>
    /// Both services write transactions — the API from the admin panel, the bot
    /// when an administrator scans a member's QR code — so both recalculate
    /// through here rather than keeping a copy of the rule each.
    /// </para>
    /// </summary>
    public static class PointsLedger
    {
        /// <summary>
        /// One <c>UPDATE ... SET "Points" = (SELECT COALESCE(SUM(...), 0) ...)</c>.
        /// Computing the balance in the database instead of reading it, adding
        /// to it and writing it back removes the lost-update race between two
        /// writers saving at the same moment.
        /// </summary>
        public static Task<int> RecalculatePointsAsync(
            this AppDbContext context,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            u => u.Points,
                            u => context.Transactions
                                .Where(t => t.UserId == userId && t.Status)
                                .Sum(t => (int?)t.Points) ?? 0)
                        .SetProperty(u => u.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }
    }
}
