using Doniyors.Data.Entities;
using backend.DTOs;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.DAL.Repositories.TravelRepo
{
    public interface ITravelRepository
    {
        /// <summary>
        /// Projected: image metadata only. Including the navigation would pull
        /// every stored image's bytes out of the database just to drop them
        /// when mapping to the DTO.
        /// </summary>
        Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked lookup of the scalar columns only, used when the adventure is
        /// about to be modified. Images are handled separately so their bytes
        /// never have to be read back.
        /// </summary>
        Task<Travel?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<Travel> AddAsync(
            Travel travel,
            CancellationToken cancellationToken = default);

        /// <summary>Ordered metadata of the images already stored for an adventure.</summary>
        Task<IReadOnlyList<TravelImageDto>> GetImageSummariesAsync(
            int travelId,
            CancellationToken cancellationToken = default);

        /// <summary>Single stored image including its bytes, used by the image endpoint.</summary>
        Task<TravelImage?> GetImageAsync(
            int imageId,
            CancellationToken cancellationToken = default);

        Task AddImagesAsync(
            IEnumerable<TravelImage> images,
            CancellationToken cancellationToken = default);

        /// <summary>Set-based delete — the rows are never loaded.</summary>
        Task DeleteImagesAsync(
            IReadOnlyCollection<int> imageIds,
            CancellationToken cancellationToken = default);

        /// <summary>Set-based reorder — the rows are never loaded.</summary>
        Task SetImageSortOrderAsync(
            int imageId,
            int sortOrder,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens a database transaction on the shared request-scoped context, so
        /// the scalar update and the image add/remove/reorder statements that go
        /// with it either all land or none do.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default);
    }
}
