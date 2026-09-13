using Doniyors.Data.Entities;
using backend.DAL.Repositories.TransactionRepo;
using backend.DAL.Repositories.TravelRepo;
using backend.DAL.Repositories.UserRepo;
using backend.DTOs;
using backend.Services.Common;

namespace backend.Services.TransactionService
{
    /// <summary>
    /// Transactions are the only writer of a user's balance. Every write here
    /// ends by recalculating <c>User.Points</c> from the surviving active
    /// transactions, inside the same database transaction, so the cached
    /// balance can never drift from the history that explains it.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITravelRepository _travelRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            ITravelRepository travelRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _travelRepository = travelRepository;
        }

        public async Task<IReadOnlyList<TransactionAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.GetAllAsync(cancellationToken);

            return transactions.Select(Map).ToList();
        }

        public async Task<TransactionAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);

            return transaction is null ? null : Map(transaction);
        }

        public async Task<SaveResult<TransactionAdminDto>> CreateAsync(
            TransactionSaveDto request,
            CancellationToken cancellationToken = default)
        {
            var invalid = await ValidateReferencesAsync(request, cancellationToken);

            if (invalid is not null)
                return invalid;

            var transaction = new Transaction
            {
                UserId = request.UserId,
                TravelId = request.TravelId,
                Points = request.Points,
                Status = request.Status,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await using var scope =
                await _transactionRepository.BeginTransactionAsync(cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            await _userRepository.RecalculatePointsAsync(request.UserId, cancellationToken);

            await scope.CommitAsync(cancellationToken);

            var created = await _transactionRepository.GetByIdAsync(
                transaction.Id,
                cancellationToken);

            return SaveResult<TransactionAdminDto>.Success(
                Map(created ?? transaction));
        }

        public async Task<SaveResult<TransactionAdminDto>> UpdateAsync(
            int id,
            TransactionSaveDto request,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepository.GetForUpdateAsync(
                id,
                cancellationToken);

            if (transaction is null)
                return SaveResult<TransactionAdminDto>.NotFound("Transaction not found.");

            var invalid = await ValidateReferencesAsync(request, cancellationToken);

            if (invalid is not null)
                return invalid;

            // Moving a transaction between users changes two balances.
            var previousUserId = transaction.UserId;

            transaction.UserId = request.UserId;
            transaction.TravelId = request.TravelId;
            transaction.Points = request.Points;
            transaction.Status = request.Status;

            await using var scope =
                await _transactionRepository.BeginTransactionAsync(cancellationToken);

            await _transactionRepository.SaveChangesAsync(cancellationToken);

            await _userRepository.RecalculatePointsAsync(request.UserId, cancellationToken);

            if (previousUserId != request.UserId)
            {
                await _userRepository.RecalculatePointsAsync(previousUserId, cancellationToken);
            }

            await scope.CommitAsync(cancellationToken);

            var updated = await _transactionRepository.GetByIdAsync(id, cancellationToken);

            return SaveResult<TransactionAdminDto>.Success(Map(updated ?? transaction));
        }

        public async Task<TransactionAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepository.GetForUpdateAsync(
                id,
                cancellationToken);

            if (transaction is null)
                return null;

            // Soft delete — the record is preserved, only the flag changes.
            // A voided transaction stops counting towards the balance.
            if (transaction.Status)
            {
                transaction.Status = false;

                await using var scope =
                    await _transactionRepository.BeginTransactionAsync(cancellationToken);

                await _transactionRepository.SaveChangesAsync(cancellationToken);

                await _userRepository.RecalculatePointsAsync(
                    transaction.UserId,
                    cancellationToken);

                await scope.CommitAsync(cancellationToken);
            }

            return Map(transaction);
        }

        /// <summary>Returns a failed result when the user or travel does not exist.</summary>
        private async Task<SaveResult<TransactionAdminDto>?> ValidateReferencesAsync(
            TransactionSaveDto request,
            CancellationToken cancellationToken)
        {
            if (!await _userRepository.ExistsAsync(request.UserId, cancellationToken))
            {
                return SaveResult<TransactionAdminDto>.Conflict(
                    ErrorCodes.UserNotFound,
                    "The selected user does not exist.");
            }

            // The adventure is optional: a transaction can be a manual adjustment.
            if (request.TravelId is int travelId
                && !await _travelRepository.ExistsAsync(travelId, cancellationToken))
            {
                return SaveResult<TransactionAdminDto>.Conflict(
                    ErrorCodes.AdventureNotFound,
                    "The selected adventure does not exist.");
            }

            return null;
        }

        private static TransactionAdminDto Map(Transaction transaction) =>
            new()
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                UserName = transaction.User?.UserName ?? string.Empty,
                TgUserId = transaction.User?.TgUserId ?? 0,
                TravelId = transaction.TravelId,
                TravelTitle = transaction.Travel?.Title ?? string.Empty,
                Points = transaction.Points,
                CreatedAt = transaction.CreatedAt,
                Status = transaction.Status,
            };
    }
}
