using backend.DTOs;
using backend.Infrastructure;
using backend.Services.Common;
using backend.Services.TravelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Policy = AuthorizationPolicies.Administrative)]
    public class TravelsController : ControllerBase
    {
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedImageTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
        ];

        private readonly ITravelService _travelService;

        public TravelsController(ITravelService travelService)
        {
            _travelService = travelService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TravelAdminDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var travels = await _travelService.GetAllAsync(cancellationToken);

            return Ok(travels);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TravelAdminDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var travel = await _travelService.GetByIdAsync(id, cancellationToken);

            if (travel is null)
                return NotFound();

            return Ok(travel);
        }

        [HttpPost]
        public async Task<ActionResult<TravelAdminDto>> Create(
            [FromForm] TravelFormDto request,
            CancellationToken cancellationToken)
        {
            var (uploads, uploadError) = await ReadUploadsAsync(
                request.Images,
                cancellationToken);

            if (uploadError is not null)
                return BadRequest(uploadError);

            var result = await _travelService.CreateAsync(
                request.ToSaveDto(),
                uploads,
                cancellationToken);

            if (result.Outcome == SaveOutcome.Conflict)
                return Conflict(ErrorBody(result));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TravelAdminDto>> Update(
            int id,
            [FromForm] TravelFormDto request,
            CancellationToken cancellationToken)
        {
            var (uploads, uploadError) = await ReadUploadsAsync(
                request.Images,
                cancellationToken);

            if (uploadError is not null)
                return BadRequest(uploadError);

            var result = await _travelService.UpdateAsync(
                id,
                request.ToSaveDto(),
                uploads,
                request.KeepImageIds,
                cancellationToken);

            return result.Outcome switch
            {
                SaveOutcome.NotFound => NotFound(),
                SaveOutcome.Conflict => Conflict(ErrorBody(result)),
                _ => Ok(result.Value),
            };
        }

        /// <summary>
        /// Soft delete. The adventure is never removed from the database —
        /// its <c>Status</c> is set to <c>false</c> so it stops being offered.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TravelAdminDto>> Disable(
            int id,
            CancellationToken cancellationToken)
        {
            var disabled = await _travelService.DisableAsync(id, cancellationToken);

            if (disabled is null)
                return NotFound();

            return Ok(disabled);
        }

        /// <summary>The image limit travels with the error so the client's message can name it.</summary>
        private static object ErrorBody(SaveResult<TravelAdminDto> result) =>
            new { code = result.Code, message = result.Error, max = TravelService.MaxImages };

        /// <summary>
        /// Reads the posted files into memory, rejecting anything that is not a
        /// reasonably sized image. Every file is checked before any of them is
        /// read, so an oversized upload at the end of the list cannot make the
        /// server buffer the ones before it first.
        /// </summary>
        private static async Task<(IReadOnlyList<TravelImageUpload> Uploads, object? Error)>
            ReadUploadsAsync(
                List<IFormFile> files,
                CancellationToken cancellationToken)
        {
            var accepted = new List<IFormFile>(files.Count);

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                if (file.Length > MaxImageBytes)
                {
                    return ([], new
                    {
                        code = ErrorCodes.ImageTooLarge,
                        message = $"\"{file.FileName}\" is larger than 5 MB.",
                        fileName = file.FileName,
                    });
                }

                if (!AllowedImageTypes.Contains(file.ContentType))
                {
                    return ([], new
                    {
                        code = ErrorCodes.ImageTypeNotAllowed,
                        message = $"\"{file.FileName}\" is not a JPEG, PNG, WebP or GIF image.",
                        fileName = file.FileName,
                    });
                }

                accepted.Add(file);
            }

            var uploads = new List<TravelImageUpload>(accepted.Count);

            foreach (var file in accepted)
            {
                using var buffer = new MemoryStream((int)file.Length);

                await file.CopyToAsync(buffer, cancellationToken);

                uploads.Add(
                    new TravelImageUpload(
                        Path.GetFileName(file.FileName),
                        file.ContentType,
                        buffer.ToArray()));
            }

            return (uploads, null);
        }
    }
}
