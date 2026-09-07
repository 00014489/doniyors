using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.DAL.Repositories.TravelRepo
{
    public class TravelRepository: ITravelRepository
    {
        private readonly AppDbContext _context;

        public TravelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Travel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Travels
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Travel?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Travels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}