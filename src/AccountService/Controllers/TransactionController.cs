using AccountService.Features.Transactions.Commands;
using AccountService.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateTransaction([FromBody] CreateTransactionCommand command)
    {
        var result = await mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetReceipt), new { transactionId = result.Value.Id }, result.Value);
        return StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("{transactionId:guid}/receipt")]
    public async Task<ActionResult> GetReceipt(Guid transactionId)
    {
        var result = await mediator.Send(new GetReceiptQuery(transactionId));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}