using AccountService.Features.Transactions.Commands;
using AccountService.Features.Transactions.Queries;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(
        [FromBody] CreateTransactionCommand command)
    {
        var transaction = await mediator.Send(command);
        return CreatedAtAction(nameof(GetReceipt), new { transactionId = transaction.Id }, transaction);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferCommand command)
    {
        var success = await mediator.Send(command);
        return success ? Ok() : BadRequest("Ошибка передачи");
    }

    [HttpGet("{transactionId:guid}/receipt")]
    public async Task<ActionResult<string>> GetReceipt(Guid transactionId) => Ok(await mediator.Send(new GetReceiptQuery(transactionId)));
}