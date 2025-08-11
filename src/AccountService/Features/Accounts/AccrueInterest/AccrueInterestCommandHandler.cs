using AccountService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Features.Accounts.AccrueInterest;

public class AccrueInterestCommandHandler(AccountServiceContext context) : IRequestHandler<AccrueInterestCommand>
{
    public async Task Handle(AccrueInterestCommand request, CancellationToken ct)
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
    }
}