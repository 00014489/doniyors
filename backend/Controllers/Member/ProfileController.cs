using backend.DTOs.Member;
using backend.Services.Common;
using backend.Services.JwtService;
using backend.Services.MemberService;
using backend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Member
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IUserService _userService;

        public ProfileController(IMemberService memberService, IUserService userService)
        {
            _memberService = memberService;
            _userService = userService;
        }

        /// <summary>The signed-in member's own profile and adventure history.</summary>
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> Get(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var profile = await _memberService.GetProfileAsync(userId, cancellationToken);

            if (profile is null)
                return NotFound();

            return Ok(profile);
        }

        /// <summary>
        /// The member's stored language. The Mini App re-reads it whenever it
        /// comes back into view, so a change made in the bot shows up without
        /// reopening the app.
        /// </summary>
        [HttpGet("language")]
        public async Task<ActionResult<LanguageDto>> GetLanguage(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var languageCode = await _userService.GetLanguageAsync(userId, cancellationToken);

            if (languageCode is null)
                return NotFound();

            return Ok(new LanguageDto { LanguageCode = languageCode });
        }

        /// <summary>
        /// The member's own choice in the Mini App. Written to the same column the
        /// bot reads, and the bot's menu button is relabelled to match.
        /// </summary>
        [HttpPut("language")]
        public async Task<ActionResult<LanguageDto>> SetLanguage(
            [FromBody] LanguageDto request,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userService.SetLanguageAsync(
                userId,
                request.LanguageCode,
                cancellationToken);

            return result.Outcome switch
            {
                SaveOutcome.NotFound => NotFound(),
                SaveOutcome.Conflict => BadRequest(new { code = result.Code, message = result.Error }),
                _ => Ok(result.Value),
            };
        }
    }
}
