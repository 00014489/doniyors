using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.TravelService
{
    public interface ITravelService
    {
        Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}