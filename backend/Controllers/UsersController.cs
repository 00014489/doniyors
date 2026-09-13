using backend.DTOs;
using backend.Services.JwtService;
using backend.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("qr-code")]
        public async Task<ActionResult<QRCodeDto>> GetQrCode(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userService.GetQrCodeAsync(userId, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// The caller's own history in full; anyone else's is limited to the
        /// current season.
        /// </summary>
        [HttpGet("transactions/{tgUserId:long}")]
        public async Task<ActionResult<IReadOnlyList<UserTransactionDto>>> GetUserTransactions(
            long tgUserId,
            CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userService.GetUserTransactionsAsync(
                tgUserId,
                userId,
                cancellationToken);

            if (result is null)
                return Unauthorized();

            return Ok(result);
        }
    }
}
