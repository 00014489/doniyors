using backend.DTOs.Member;
using backend.Services.MemberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace backend.Controllers.Member
{
    /// <summary>
    /// The adventures the Mini App shows its members. Read-only — creating and
    /// editing lives under <c>api/admin/Travels</c>.
    /// </summary>
    [ApiController]
    [Route("api/adventures")]
    [Authorize]
    public class AdventuresController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public AdventuresController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>Upcoming adventures only, soonest first.</summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdventureListItemDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var adventures = await _memberService.GetAdventuresAsync(cancellationToken);

            return Ok(adventures);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdventureDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var adventure = await _memberService.GetAdventureAsync(id, cancellationToken);

            if (adventure is null)
                return NotFound();

            return Ok(adventure);
        }

        /// <summary>
        /// Serves one stored image. Anonymous because an <c>&lt;img&gt;</c> tag
        /// cannot send the bearer token; the id reveals nothing beyond the
        /// picture itself. This is the canonical image URL for both the member
        /// app and the admin panel.
        /// </summary>
        [HttpGet("images/{imageId:int}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetImage(
            int imageId,
            CancellationToken cancellationToken)
        {
            var image = await _memberService.GetImageAsync(imageId, cancellationToken);

            if (image is null)
                return NotFound();

            // Bytes never change once stored — a new upload gets a new id — so
            // the browser can revalidate cheaply instead of re-downloading.
            return new FileContentResult(image.Data, image.ContentType)
            {
                EntityTag = new EntityTagHeaderValue($"\"{image.Id}-{image.Data.LongLength}\""),
            };
        }
    }
}
