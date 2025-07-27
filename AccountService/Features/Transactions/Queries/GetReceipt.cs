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
            return Task.FromResult("���������� �� �������");

        return Task.FromResult($$"""
                                 =========== ������� �� ���������� ===========
                                 ID ����������: {{transaction.Id}}
                                 ����: {{transaction.DateTime:yyyy-MM-dd HH:mm:ss}}
                                 ����: {{transaction.AccountId}}
                                 ����������: {{transaction.CounterpartyAccountId?.ToString() ?? "�� ���������"}}
                                 ���: {{transaction.Type}}
                                 �����: {{transaction.Sum}} {{transaction.Currency}}
                                 ��������: {{transaction.Description}}
                                 ============================================
                                 """);
    }
}