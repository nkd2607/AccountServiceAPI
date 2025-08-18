using AccountService.Data;
using AccountService.Domain.Events;
using AccountService.Infrastructure.Outbox;
using AccountService.Models;
using AccountService.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Features.Accounts.AccrueInterest;

public class AccrueInterestCommandHandler(AccountServiceContext context, OutboxService outbox) : IRequestHandler<AccrueInterestCommand, Result<Account>>
{
    public async Task<Result<Account>> Handle(AccrueInterestCommand request, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var accountExists = await context.Accounts
                .AnyAsync(a => a.Id == request.AccountId, ct);

            if (!accountExists)
                throw new ApplicationException("—чЄт не найден");

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT accrue_interest({request.AccountId})", ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        var evt = new InterestAccrued(
            Guid.NewGuid(),
            DateTime.UtcNow,
            request.AccountId,
            request.PeriodFrom,
            request.PeriodTo,
            request.Amount
        );

        await outbox.AddAsync(evt, ct);
        return null!;
    }
}