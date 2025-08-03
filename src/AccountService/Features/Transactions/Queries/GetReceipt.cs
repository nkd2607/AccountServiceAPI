using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Transactions.Queries;

public record GetReceiptQuery(Guid TransactionId) : IRequest<Result<string>>;

public class GetReceiptHandler(IAccountStorageService storage) : IRequestHandler<GetReceiptQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetReceiptQuery request, CancellationToken token)
    {
        var transaction = storage.GetAccounts().SelectMany(a => a.Transactions)
            .FirstOrDefault(t => t.Id == request.TransactionId);
        if (transaction == null) return Task.FromResult(Result<string>.Failure("Транзакция не найдена", 404));
        var receipt =
            $"""                        ============ Выписка по транзакции ============                        ID: {transaction.Id}                        Дата: {transaction.DateTime:yyyy-MM-dd HH:mm:ss}                        Счёт: {transaction.AccountId}                        Тип: {transaction.Type}                        Сумма: {transaction.Sum} {transaction.Currency}                        Описание: {transaction.Description}                        ===============================================                        """;
        return Task.FromResult(Result<string>.Success(receipt));
    }
}