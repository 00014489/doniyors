using Doniyors.Data.Entities;
using backend.DAL.Repositories.TravelRepo;
using backend.DTOs;
using backend.Services.Common;

namespace backend.Services.TravelService
{
    public class TravelService : ITravelService
    {
        /// <summary>An adventure must always have between one and five images.</summary>
        public const int MinImages = 1;
        public const int MaxImages = 5;

        private readonly ITravelRepository _travelRepository;

        public TravelService(ITravelRepository travelRepository)
        {
            _travelRepository = travelRepository;
        }

        public Task<IReadOnlyList<TravelAdminDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetAllAsync(cancellationToken);
        }

        public Task<TravelAdminDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<SaveResult<TravelAdminDto>> CreateAsync(
            TravelSaveDto request,
            IReadOnlyList<TravelImageUpload> images,
            CancellationToken cancellationToken = default)
        {
            var countError = ValidateImageCount(images.Count);

            if (countError is not null)
                return countError;

            var travel = new Travel
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                TravelDate = request.TravelDate,
                Cost = request.Cost,
                Points = request.Points,
                Status = request.Status,
            };

            var order = 0;

            foreach (var image in images)
            {
                travel.Images.Add(ToEntity(image, order++));
            }

            await _travelRepository.AddAsync(travel, cancellationToken);
            await _travelRepository.SaveChangesAsync(cancellationToken);

            // Re-read through the projection so the response carries the ids
            // the database assigned to the new images.
            var created = await _travelRepository.GetByIdAsync(travel.Id, cancellationToken);

            return created is null
                ? SaveResult<TravelAdminDto>.Conflict(
                    ErrorCodes.SaveFailed,
                    "The adventure could not be read back after saving.")
                : SaveResult<TravelAdminDto>.Success(created);
        }

        public async Task<SaveResult<TravelAdminDto>> UpdateAsync(
            int id,
            TravelSaveDto request,
            IReadOnlyList<TravelImageUpload> newImages,
            IReadOnlyList<int> keepImageIds,
            CancellationToken cancellationToken = default)
        {
            var travel = await _travelRepository.GetForUpdateAsync(id, cancellationToken);

            if (travel is null)
                return SaveResult<TravelAdminDto>.NotFound("Adventure not found.");

            var stored = await _travelRepository.GetImageSummariesAsync(id, cancellationToken);

            var kept = stored
                .Where(image => keepImageIds.Contains(image.Id))
                .OrderBy(image => image.SortOrder)
                .ToList();

            var countError = ValidateImageCount(kept.Count + newImages.Count);

            if (countError is not null)
                return countError;

            // Anything the administrator removed from the form is dropped.
            var removedIds = stored
                .Select(image => image.Id)
                .Except(kept.Select(image => image.Id))
                .ToList();

            travel.Title = request.Title.Trim();
            travel.Description = request.Description.Trim();
            travel.TravelDate = request.TravelDate;
            travel.Cost = request.Cost;
            travel.Points = request.Points;
            travel.Status = request.Status;

            await using var scope =
                await _travelRepository.BeginTransactionAsync(cancellationToken);

            await _travelRepository.DeleteImagesAsync(removedIds, cancellationToken);

            // Close the gaps the removals left, so the cover image is always
            // the one at position zero.
            var order = 0;

            foreach (var image in kept)
            {
                if (image.SortOrder != order)
                {
                    await _travelRepository.SetImageSortOrderAsync(
                        image.Id,
                        order,
                        cancellationToken);
                }

                order++;
            }

            if (newImages.Count > 0)
            {
                await _travelRepository.AddImagesAsync(
                    newImages.Select(image => ToEntity(image, order++, travel.Id)),
                    cancellationToken);
            }

            await _travelRepository.SaveChangesAsync(cancellationToken);

            await scope.CommitAsync(cancellationToken);

            var updated = await _travelRepository.GetByIdAsync(id, cancellationToken);

            return updated is null
                ? SaveResult<TravelAdminDto>.NotFound("Adventure not found.")
                : SaveResult<TravelAdminDto>.Success(updated);
        }

        public async Task<TravelAdminDto?> DisableAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var travel = await _travelRepository.GetForUpdateAsync(id, cancellationToken);

            if (travel is null)
                return null;

            // Soft delete — the row is preserved, only the flag changes.
            if (travel.Status)
            {
                travel.Status = false;

                await _travelRepository.SaveChangesAsync(cancellationToken);
            }

            return await _travelRepository.GetByIdAsync(id, cancellationToken);
        }

        private static SaveResult<TravelAdminDto>? ValidateImageCount(int count) =>
            count switch
            {
                < MinImages => SaveResult<TravelAdminDto>.Conflict(
                    ErrorCodes.ImageRequired,
                    "An adventure needs at least one image."),

                > MaxImages => SaveResult<TravelAdminDto>.Conflict(
                    ErrorCodes.TooManyImages,
                    $"An adventure can have at most {MaxImages} images."),

                _ => null,
            };

        private static TravelImage ToEntity(
            TravelImageUpload upload,
            int sortOrder,
            int? travelId = null) =>
            new()
            {
                TravelId = travelId ?? 0,
                Title = upload.FileName,
                ContentType = upload.ContentType,
                Data = upload.Data,
                SortOrder = sortOrder,
                CreatedAt = DateTimeOffset.UtcNow,
            };
    }
}
