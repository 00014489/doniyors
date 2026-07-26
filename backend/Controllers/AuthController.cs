using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Services.AuthService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpPost("telegram")]
        public async Task<IActionResult> Login([FromBody] TelegramLoginRequest request)
        {
            var response = await _authService.LoginAsync(request.InitData);

            return Ok(response);
        }
    }
}