using Doniyors.Data.Entities;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Doniyors.Data;

namespace backend.DAL.Repositories.TravelRepo
{
    public class TravelRepository : ITravelRepository
    {
        private readonly AppDbContext _context;

        public TravelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await Project(_context.Travels.OrderByDescending(x => x.CreatedAt))
                .ToListAsync(cancellationToken);
        }

        public Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Project(_context.Travels.Where(x => x.Id == id))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<Travel?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            // No Include: the caller reads image metadata through
            // GetImageSummariesAsync, so the bytes stay in the database.
            return _context.Travels
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Travel> AddAsync(
            Travel travel,
            CancellationToken cancellationToken = default)
        {
            await _context.Travels.AddAsync(travel, cancellationToken);

            return travel;
        }

        public async Task<IReadOnlyList<TravelImageDto>> GetImageSummariesAsync(
            int travelId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TravelImages
                .AsNoTracking()
                .Where(x => x.TravelId == travelId)
                .OrderBy(x => x.SortOrder)
                .Select(x => new TravelImageDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    ContentType = x.ContentType,
                    SortOrder = x.SortOrder,
                })
                .ToListAsync(cancellationToken);
        }

        public Task<TravelImage?> GetImageAsync(
            int imageId,
            CancellationToken cancellationToken = default)
        {
            return _context.TravelImages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        }

        public async Task AddImagesAsync(
            IEnumerable<TravelImage> images,
            CancellationToken cancellationToken = default)
        {
            await _context.TravelImages.AddRangeAsync(images, cancellationToken);
        }

        public Task DeleteImagesAsync(
            IReadOnlyCollection<int> imageIds,
            CancellationToken cancellationToken = default)
        {
            if (imageIds.Count == 0)
                return Task.CompletedTask;

            return _context.TravelImages
                .Where(x => imageIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        public Task SetImageSortOrderAsync(
            int imageId,
            int sortOrder,
            CancellationToken cancellationToken = default)
        {
            return _context.TravelImages
                .Where(x => x.Id == imageId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.SortOrder, sortOrder),
                    cancellationToken);
        }

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _context.Travels.AnyAsync(x => x.Id == id, cancellationToken);
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

        /// <summary>Shared shape for the list and detail endpoints — metadata, no bytes.</summary>
        private static IQueryable<TravelAdminDto> Project(IQueryable<Travel> query) =>
            query
                .AsNoTracking()
                .Select(travel => new TravelAdminDto
                {
                    Id = travel.Id,
                    Title = travel.Title,
                    Description = travel.Description,
                    TravelDate = travel.TravelDate,
                    Cost = travel.Cost,
                    Points = travel.Points,
                    CreatedAt = travel.CreatedAt,
                    Status = travel.Status,
                    Images = travel.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => new TravelImageDto
                        {
                            Id = image.Id,
                            Title = image.Title,
                            ContentType = image.ContentType,
                            SortOrder = image.SortOrder,
                        })
                        .ToList(),
                });
    }
}
