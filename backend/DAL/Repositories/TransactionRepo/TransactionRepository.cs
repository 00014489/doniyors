using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Doniyors.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Doniyors.Data;

namespace backend.DAL.Repositories.TransactionRepo
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Transaction>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Travel)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Transaction?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Travel)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Transaction?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Travel)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Transaction> AddAsync(
            Transaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);

            return transaction;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
