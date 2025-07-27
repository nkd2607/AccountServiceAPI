using AccountService.Features.Accounts.Commands;
using AccountService.Features.Accounts.Queries;
using AccountService.Features.Transactions.Commands;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount(
        [FromBody] CreateAccountCommand command)
    {
        var account = await mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts([FromQuery] GetAccountsQuery query) => Ok(await mediator.Send(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Account>> GetAccount(Guid id)
    {
        var account = await mediator.Send(new GetAccountByIdQuery(id));
        return account == null ? NotFound() : Ok(account);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
    {
        if (id != command.Id) return BadRequest("Несовпадение ID");
        var result = await mediator.Send(command);
        return result == null ? NotFound() : NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var success = await mediator.Send(new DeleteAccountCommand(id));
        return success ? NoContent() : NotFound();
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<ActionResult<bool>> ClientHasAccount(Guid clientId) => Ok(await mediator.Send(new ClientHasAccountQuery(clientId)));

    [HttpPost("{id:guid}/transactions")]
    public async Task<ActionResult<Transaction>> CreateTransaction(Guid id, [FromBody] CreateTransactionCommand command)
    {
        if (id != command.AccountId) return BadRequest("Несовпадение ID счёта");
        var transaction = await mediator.Send(command);
        return CreatedAtAction(nameof(TransactionsController.GetReceipt), "Transactions", new { transactionId = transaction.Id }, transaction);
    }
}