using backend.DTOs.Member;
using backend.Services.JwtService;
using backend.Services.MemberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Member
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public LeaderboardController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// Rankings for one season, plus the list of seasons to choose from.
        /// Omitting <paramref name="season"/> selects the current one.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<LeaderboardDto>> Get(
            [FromQuery] string? season,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var leaderboard = await _memberService.GetLeaderboardAsync(
                season,
                userId,
                cancellationToken);

            return Ok(leaderboard);
        }
    }
}
