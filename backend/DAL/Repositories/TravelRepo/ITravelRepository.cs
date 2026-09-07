using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Entities;

namespace backend.DAL.Repositories.TravelRepo
{
    public interface ITravelRepository
    {
        Task<IReadOnlyList<Travel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Travel?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}