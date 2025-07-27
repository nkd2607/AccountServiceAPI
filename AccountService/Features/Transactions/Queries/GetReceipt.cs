using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Transactions.Queries;

public record GetReceiptQuery(Guid TransactionId) : IRequest<string>;

public class GetReceiptHandler(IAccountStorageService accountStorage) : IRequestHandler<GetReceiptQuery, string>
{
    public Task<string> Handle(GetReceiptQuery request, CancellationToken token)
    {
        var transaction = accountStorage.GetAccounts()
            .SelectMany(a => a.Transactions)
            .FirstOrDefault(t => t.Id == request.TransactionId);

        if (transaction == null)
            return Task.FromResult("Транзакция не найдена");

        return Task.FromResult($$"""
                                 =========== ВЫПИСКА ПО ТРАНЗАКЦИИ ===========
                                 ID Транзакции: {{transaction.Id}}
                                 Дата: {{transaction.DateTime:yyyy-MM-dd HH:mm:ss}}
                                 Счёт: {{transaction.AccountId}}
                                 Контрагент: {{transaction.CounterpartyAccountId?.ToString() ?? "Не обнаружен"}}
                                 Тип: {{transaction.Type}}
                                 Сумма: {{transaction.Sum}} {{transaction.Currency}}
                                 Описание: {{transaction.Description}}
                                 ============================================
                                 """);
    }
}