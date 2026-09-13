using backend.Infrastructure;
using backend.DTOs;
using backend.Services.Common;
using backend.Services.UserAdminService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/Users")]
    [Authorize(Policy = AuthorizationPolicies.Administrative)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserAdminService _userAdminService;

        public AdminUsersController(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserAdminDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var users = await _userAdminService.GetAllAsync(cancellationToken);

            return Ok(users);
        }

        /// <summary>Selectable account types, used to populate the user form.</summary>
        [HttpGet("roles")]
        public async Task<ActionResult<IReadOnlyList<UserRoleDto>>> GetRoles(
            CancellationToken cancellationToken)
        {
            var roles = await _userAdminService.GetRolesAsync(cancellationToken);

            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserAdminDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await _userAdminService.GetByIdAsync(id, cancellationToken);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserAdminDto>> Create(
            [FromBody] UserSaveDto request,
            CancellationToken cancellationToken)
        {
            var result = await _userAdminService.CreateAsync(request, cancellationToken);

            if (result.Outcome == SaveOutcome.Conflict)
                return Conflict(new { code = result.Code, message = result.Error });

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserAdminDto>> Update(
            int id,
            [FromBody] UserSaveDto request,
            CancellationToken cancellationToken)
        {
            var result = await _userAdminService.UpdateAsync(id, request, cancellationToken);

            return result.Outcome switch
            {
                SaveOutcome.NotFound => NotFound(),
                SaveOutcome.Conflict => Conflict(new { code = result.Code, message = result.Error }),
                _ => Ok(result.Value)
            };
        }

        /// <summary>
        /// Soft delete. The user is never removed from the database —
        /// their <c>Status</c> is set to <c>false</c> so the account is disabled.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<UserAdminDto>> Disable(
            int id,
            CancellationToken cancellationToken)
        {
            var disabled = await _userAdminService.DisableAsync(id, cancellationToken);

            if (disabled is null)
                return NotFound();

            return Ok(disabled);
        }
    }
}
