using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record CreateAccountCommand(Guid OwnerId, AccountType Type, string Currency, decimal? InterestRate) : IRequest<Account>;

public class CreateAccountHandler(IAccountStorageService storage, IClientVerificationService clientVerification, ICurrencyService currencyService) : IRequestHandler<CreateAccountCommand, Account>
{
    public Task<Account>  Handle(CreateAccountCommand request, CancellationToken token)
    {
        if (!clientVerification.VerifyClientExists(request.OwnerId))
            throw new Exception("������ �� ������");

        if (!currencyService.IsCurrencySupported(request.Currency))
            throw new Exception("������ �� ��������������");

        if (request.Type != AccountType.Checking && request.InterestRate == null)
            throw new Exception("��� �������� ������/������� ��������� ���������� ������");

        var account = new Account
        {
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            Balance = 0,
            InterestRate = request.InterestRate,
            OpeningDate = DateTime.UtcNow
        };

        return Task.FromResult(storage.CreateAccount(account));
    }
}