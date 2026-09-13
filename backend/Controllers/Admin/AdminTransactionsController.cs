using backend.Infrastructure;
using backend.DTOs;
using backend.Services.Common;
using backend.Services.TransactionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/Transactions")]
    [Authorize(Policy = AuthorizationPolicies.Administrative)]
    public class AdminTransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public AdminTransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TransactionAdminDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var transactions = await _transactionService.GetAllAsync(cancellationToken);

            return Ok(transactions);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TransactionAdminDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var transaction = await _transactionService.GetByIdAsync(id, cancellationToken);

            if (transaction is null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionAdminDto>> Create(
            [FromBody] TransactionSaveDto request,
            CancellationToken cancellationToken)
        {
            var result = await _transactionService.CreateAsync(request, cancellationToken);

            if (result.Outcome == SaveOutcome.Conflict)
                return Conflict(new { code = result.Code, message = result.Error });

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TransactionAdminDto>> Update(
            int id,
            [FromBody] TransactionSaveDto request,
            CancellationToken cancellationToken)
        {
            var result = await _transactionService.UpdateAsync(id, request, cancellationToken);

            return result.Outcome switch
            {
                SaveOutcome.NotFound => NotFound(),
                SaveOutcome.Conflict => Conflict(new { code = result.Code, message = result.Error }),
                _ => Ok(result.Value)
            };
        }

        /// <summary>
        /// Soft delete. The transaction is never removed from the database —
        /// its <c>Status</c> is set to <c>false</c> so it reads as voided.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TransactionAdminDto>> Disable(
            int id,
            CancellationToken cancellationToken)
        {
            var disabled = await _transactionService.DisableAsync(id, cancellationToken);

            if (disabled is null)
                return NotFound();

            return Ok(disabled);
        }
    }
}
