using Doniyors.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.DAL.Repositories.TransactionRepo
{
    public interface ITransactionRepository
    {
        Task<IReadOnlyList<Transaction>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Transaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>Tracked lookup, used when the transaction is about to be modified.</summary>
        Task<Transaction?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Transaction> AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens a database transaction on the shared request-scoped context, so
        /// a write and the balance recalculation that follows it either both
        /// land or neither does. Every repository in the request enlists in it.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default);
    }
}
