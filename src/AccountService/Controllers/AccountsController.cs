using AccountService.Features.Accounts.Commands;
using AccountService.Features.Accounts.Queries;
using AccountService.Features.Transactions.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await mediator.Send(command);
        if (result.IsSuccess) return CreatedAtAction(nameof(GetAccount), new { id = result.Value.Id }, result.Value);
        return StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet]
    public async Task<ActionResult> GetAccounts([FromQuery] GetAccountsQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetAccount(Guid id)
    {
        var result = await mediator.Send(new GetAccountByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
    {
        if (id != command.Id) return BadRequest(new { error = "Несовпадение ID" });
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAccount(Guid id)
    {
        var result = await mediator.Send(new DeleteAccountCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<ActionResult> ClientHasAccount(Guid clientId)
    {
        var result = await mediator.Send(new ClientHasAccountQuery(clientId));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{id:guid}/transactions")]
    public async Task<ActionResult> CreateTransaction(Guid id, [FromBody] CreateTransactionCommand command)
    {
        if (id != command.AccountId) return BadRequest(new { error = "Несовпадение ID счёта" });
        var result = await mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(TransactionsController.GetReceipt), "Transactions",
                new { transactionId = result.Value.Id }, result.Value);
        return StatusCode(result.StatusCode, new { error = result.Error });
    }
}