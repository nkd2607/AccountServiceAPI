using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Transactions.Commands;

public record TransferCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    string Description
) : IRequest<bool>;

public class TransferHandler(IAccountStorageService storage, ICurrencyService currencyService) : IRequestHandler<TransferCommand, bool>
{
    public Task<bool> Handle(TransferCommand request, CancellationToken token)
    {
        if (!currencyService.IsCurrencySupported(request.Currency))
            throw new Exception("Валюта не поддерживается");

        var fromAccount = storage.GetAccount(request.FromAccountId);
        var toAccount = storage.GetAccount(request.ToAccountId);

        if (fromAccount == null || toAccount == null)
            return Task.FromResult(false);

        if (fromAccount.Balance < request.Amount)
            return Task.FromResult(false);

        var debitTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            CounterpartyAccountId = toAccount.Id,
            Sum = request.Amount,
            Currency = request.Currency,
            Type = TransactionType.Debit,
            Description = request.Description,
            DateTime = DateTime.UtcNow
        };

        var creditTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            CounterpartyAccountId = fromAccount.Id,
            Sum = request.Amount,
            Currency = request.Currency,
            Type = TransactionType.Credit,
            Description = request.Description,
            DateTime = DateTime.UtcNow
        };

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        fromAccount.Transactions.Add(debitTransaction);
        toAccount.Transactions.Add(creditTransaction);

        storage.UpdateAccount(fromAccount);
        storage.UpdateAccount(toAccount);

        return Task.FromResult(true);
    }
}