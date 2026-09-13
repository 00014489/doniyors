
using backend.DTOs;
using backend.Services.Common;

namespace backend.Services.TravelService
{
    public interface ITravelService
    {
        Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<SaveResult<TravelAdminDto>> CreateAsync(
            TravelSaveDto request,
            IReadOnlyList<TravelImageUpload> images,
            CancellationToken cancellationToken = default);

        Task<SaveResult<TravelAdminDto>> UpdateAsync(
            int id,
            TravelSaveDto request,
            IReadOnlyList<TravelImageUpload> newImages,
            IReadOnlyList<int> keepImageIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete: flips <c>Status</c> to <c>false</c> instead of removing the row.
        /// Returns <c>null</c> when the adventure does not exist.
        /// </summary>
        Task<TravelAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
