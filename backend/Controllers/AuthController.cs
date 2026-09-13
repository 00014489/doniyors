using backend.DTOs;
using backend.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Exchanges Telegram Mini App initData for a JWT. A rejected signature
        /// throws, and the global handler turns that into a 401.
        /// </summary>
        [HttpPost("telegram")]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] TelegramLoginRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request.InitData, cancellationToken);

            return Ok(response);
        }
    }
}
