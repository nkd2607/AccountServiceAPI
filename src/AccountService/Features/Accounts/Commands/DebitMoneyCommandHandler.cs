using AccountService.Data;
using AccountService.Domain.Events;
using AccountService.Infrastructure.Outbox;
using AccountService.Models;
using AccountService.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Features.Accounts.Commands;

public record DebitMoneyCommand(Guid AccountId, decimal Amount, string Reason, Guid OperationId)
    : IRequest<Result<Account>>;

public class DebitMoneyCommandHandler(
    AccountServiceContext context,
    OutboxService outbox,
    ILogger<DebitMoneyCommandHandler> logger)
    : IRequestHandler<DebitMoneyCommand, Result<Account>>
{
    public async Task<Result<Account>> Handle(DebitMoneyCommand request, CancellationToken ct)
    {
        await using var tx = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var account = await context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, ct);

            if (account is null)
                return Result<Account>.Failure("Счёт не найден", 404);

            if (account.Frozen) 
                return Result<Account>.Failure("Счёт был заморожен", 409);

            if (account.Balance < request.Amount)
                return Result<Account>.Failure("Недостаточно средств", 409);

            account.Balance -= request.Amount;

            var evt = new MoneyDebited(
                Guid.NewGuid(),
                DateTime.UtcNow,
                account.Id,
                request.Amount,
                account.Currency,
                request.OperationId,
                request.Reason
            );

            await outbox.AddAsync(evt, ct);
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            logger.LogInformation("Debited {Amount} from {AccountId}", request.Amount, request.AccountId);
            return Result<Account>.Success(account, 202);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            logger.LogError(ex, "Debit failed");
            throw;
        }
    }
}