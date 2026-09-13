using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Services.Common;

namespace backend.Services.TransactionService
{
    public interface ITransactionService
    {
        Task<IReadOnlyList<TransactionAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TransactionAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<SaveResult<TransactionAdminDto>> CreateAsync(
            TransactionSaveDto request,
            CancellationToken cancellationToken = default);

        Task<SaveResult<TransactionAdminDto>> UpdateAsync(
            int id,
            TransactionSaveDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete: flips <c>Status</c> to <c>false</c> so the record is kept
        /// for auditing instead of being removed.
        /// </summary>
        Task<TransactionAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
