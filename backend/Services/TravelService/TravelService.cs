using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DAL.Repositories.TravelRepo;
using backend.DTOs;

namespace backend.Services.TravelService
{
    public class TravelService: ITravelService
    {
        private readonly ITravelRepository _travelRepository;

        public TravelService(ITravelRepository travelRepository)
        {
            _travelRepository = travelRepository;
        }

        public async Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var travels = await _travelRepository.GetAllAsync(cancellationToken);

            return travels
                .Select(x => new TravelAdminDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    TravelDate = x.TravelDate,
                    Cost = x.Cost,
                    Points = x.Points,
                    CreatedAt = x.CreatedAt
                })
                .ToList();
        }

        public async Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var travel = await _travelRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (travel is null)
                return null;

            return new TravelAdminDto
            {
                Id = travel.Id,
                Title = travel.Title,
                TravelDate = travel.TravelDate,
                Cost = travel.Cost,
                Points = travel.Points,
                CreatedAt = travel.CreatedAt
            };
        }
    }
}