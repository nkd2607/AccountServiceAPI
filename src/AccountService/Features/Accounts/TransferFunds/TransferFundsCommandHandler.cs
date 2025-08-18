using AccountService.Data;
using AccountService.Models;
using AccountService.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace AccountService.Features.Accounts.TransferFunds;

public class TransferFundsCommandHandler(
    AccountServiceContext context,
    ILogger<TransferFundsCommandHandler> logger)
    : IRequestHandler<TransferFundsCommand>
{
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 200;

    public async Task Handle(TransferFundsCommand request, CancellationToken ct)
    {
        int retryCount = 0;
        bool succeeded = false;

        while (!succeeded && retryCount <= MaxRetries)
        {
            try
            {
                await ExecuteTransfer(request, ct);
                succeeded = true;
            }
            catch (Exception ex) when (IsTransientError(ex) && retryCount < MaxRetries)
            {
                retryCount++;
                var delay = TimeSpan.FromMilliseconds(InitialDelayMs * Math.Pow(2, retryCount));
                logger.LogWarning(ex, "Transfer failed. Retry #{RetryCount} in {Delay}ms", retryCount, delay.TotalMilliseconds);
                await Task.Delay(delay, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Transfer failed after {RetryCount} retries", retryCount);
                throw;
            }
        }
    }

    private async Task ExecuteTransfer(TransferFundsCommand request, CancellationToken ct)
    {
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var fromAccount = await LockAccount(request.FromAccountId, ct)
                ?? throw new ApplicationException("Source account not found");

            var toAccount = await LockAccount(request.ToAccountId, ct)
                ?? throw new ApplicationException("Target account not found");

            ValidateTransfer(fromAccount, toAccount, request.Amount);

            var fromBalanceBefore = fromAccount.Balance;
            var toBalanceBefore = toAccount.Balance;

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            RecordTransactions(fromAccount, toAccount, request.Amount);

            await context.SaveChangesAsync(ct);

            await VerifyBalances(fromAccount.Id, toAccount.Id,
                fromBalanceBefore, toBalanceBefore,
                request.Amount, ct);

            await transaction.CommitAsync(ct);
            logger.LogInformation("Transfer succeeded: {Amount} from {From} to {To}",
                request.Amount, request.FromAccountId, request.ToAccountId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Transfer operation failed");
            throw;
        }
    }

    private async Task<Account?> LockAccount(Guid accountId, CancellationToken ct)
    {
        return await context.Accounts
            .FromSqlInterpolated(
                $"SELECT * FROM \"Accounts\" WHERE \"Id\" = {accountId} FOR UPDATE")
            .SingleOrDefaultAsync(ct);
    }

    private void RecordTransactions(Account fromAccount, Account toAccount, decimal amount)
    {
        context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            CounterpartyAccountId = toAccount.Id,
            Sum = -amount,
            Currency = fromAccount.Currency,
            Type = TransactionType.Transfer,
            Description = $"Transfer to {toAccount.Id}",
            DateTime = DateTime.UtcNow
        });

        context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            CounterpartyAccountId = fromAccount.Id,
            Sum = amount,
            Currency = toAccount.Currency,
            Type = TransactionType.Transfer,
            Description = $"Transfer from {fromAccount.Id}",
            DateTime = DateTime.UtcNow
        });
    }

    private void ValidateTransfer(Account from, Account to, decimal amount)
    {
        if (from.Currency != to.Currency)
            throw new ApplicationException("Currency mismatch");

        if (from.Balance < amount)
            throw new ApplicationException("Insufficient funds");

        if (from.ClosingDate.HasValue || to.ClosingDate.HasValue)
            throw new ApplicationException("Account is closed");

        if (amount <= 0)
            throw new ApplicationException("Invalid transfer amount");
    }

    private async Task VerifyBalances(Guid fromAccountId,
        Guid toAccountId,
        decimal fromBalanceBefore,
        decimal toBalanceBefore,
        decimal amount,
        CancellationToken ct)
    {
        var fromBalanceAfter = await context.Database
            .SqlQuery<decimal>($"SELECT \"Balance\" FROM \"Accounts\" WHERE \"Id\" = {fromAccountId}")
            .SingleAsync(ct);

        var toBalanceAfter = await context.Database
            .SqlQuery<decimal>($"SELECT \"Balance\" FROM \"Accounts\" WHERE \"Id\" = {toAccountId}")
            .SingleAsync(ct);

        var expectedFromBalance = fromBalanceBefore - amount;
        var expectedToBalance = toBalanceBefore + amount;

        if (fromBalanceAfter != expectedFromBalance || toBalanceAfter != expectedToBalance)
        {
            throw new ApplicationException(
                $"Balance verification failed! " +
                $"Expected: From={expectedFromBalance}, To={expectedToBalance}. " +
                $"Actual: From={fromBalanceAfter}, To={toBalanceAfter}");
        }
    }

    private bool IsTransientError(Exception ex)
    {
        if (ex is NpgsqlException npgEx)
        {
            return npgEx.IsTransient ||
                   npgEx.SqlState == "40001" ||
                   npgEx.SqlState == "40P01" ||
                   npgEx.SqlState!.StartsWith("08");
        }

        if (ex is DbUpdateException dbUpdateEx)
        {
            return dbUpdateEx.InnerException is NpgsqlException innerNpgEx &&
                   IsTransientError(innerNpgEx);
        }

        return false;
    }
}