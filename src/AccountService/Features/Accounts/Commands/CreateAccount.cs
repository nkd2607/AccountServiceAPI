using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record CreateAccountCommand(Guid OwnerId, AccountType Type, string Currency, decimal? InterestRate)
    : IRequest<Result<Account>>;

public class CreateAccountHandler(
    IAccountStorageService storage,
    IClientVerificationService clientVerification,
    ICurrencyService currencyService) : IRequestHandler<CreateAccountCommand, Result<Account>>
{
    public Task<Result<Account>> Handle(CreateAccountCommand request, CancellationToken token)
    {
        if (!clientVerification.VerifyClientExists(request.OwnerId))
            return Task.FromResult(Result<Account>.Failure("Клиент не найден", 404));
        if (!currencyService.IsCurrencySupported(request.Currency))
            return Task.FromResult(Result<Account>.Failure("Валюта не поддерживается"));
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            return Task.FromResult(Result<Account>.Failure("Для счетов кредитов/вкладов необходима процентная ставка"));
        var account = new Account
        {
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            Balance = 0,
            InterestRate = request.InterestRate,
            OpeningDate = DateTime.UtcNow
        };
        var createdAccount = storage.CreateAccount(account);
        return Task.FromResult(Result<Account>.Success(createdAccount, 201));
    }
}