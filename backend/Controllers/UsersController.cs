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
        public async Task<IActionResult> GetQrCode()
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _userService.GetQrCodeAsync(userId);

            if (result == null)
                return Unauthorized();
            // Console.WriteLine($"UserId: {userId}, QrToken: {result.QrToken}, Points: {result.Points}");
            return Ok(result);
        }
        // [HttpGet("transactions/{tgUserId:long}")]
        // public async Task<IActionResult> GetUserTransactions(long tgUserId)
        // {
        //     var userIdClaim = User.FindFirst("UserId");

        //     if (userIdClaim is null)
        //         return Unauthorized();

        //     int userId = int.Parse(userIdClaim.Value);

        //     var result = await _userService.GetUserTransactionsAsync(tgUserId, userId);

        //     return Ok(result);
        // }
    }
}