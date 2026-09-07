using backend.DTOs;
using backend.Services.TravelService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TravelsController : ControllerBase
    {
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
            var travel = await _travelService.GetByIdAsync(
                id,
                cancellationToken);

            if (travel is null)
                return NotFound();

            return Ok(travel);
        }
    }
}