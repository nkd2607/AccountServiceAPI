using AccountService.Models.Enums;
using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;
using Transaction = AccountService.Models.Transaction;

namespace AccountService.Features.Transactions.Commands;

public record CreateTransactionCommand(
    Guid AccountId,
    decimal Sum,
    string Currency,
    TransactionType Type,
    string Description,
    Guid? CounterpartyAccountId = null) : IRequest<Result<Transaction>>;

public class CreateTransactionHandler(IAccountStorageService storage, ICurrencyService currencyService)
    : IRequestHandler<CreateTransactionCommand, Result<Transaction>>
{
    public Task<Result<Transaction>> Handle(CreateTransactionCommand request, CancellationToken token)
    {
        if (!currencyService.IsCurrencySupported(request.Currency)) throw new Exception("Валюта не поддерживается");
        var account = storage.GetAccount(request.AccountId);
        if (account == null) throw new Exception("Счёт не найден");
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(), AccountId = request.AccountId, CounterpartyAccountId = request.CounterpartyAccountId,
            Sum = request.Sum, Currency = request.Currency, Type = request.Type, Description = request.Description,
            DateTime = DateTime.UtcNow
        };
        if (request.Type == TransactionType.Credit) account.Balance += request.Sum;
        else account.Balance -= request.Sum;
        account.Transactions.Add(transaction);
        storage.UpdateAccount(account);
        return Task.FromResult(Result<Transaction>.Success(transaction));
    }
}