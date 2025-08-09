using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record UpdateAccountCommand(
    Guid Id,
    Guid OwnerId,
    AccountType Type,
    string Currency,
    decimal Balance,
    decimal? InterestRate,
    DateTime? ClosingDate) : IRequest<Result<Account>>;

public class UpdateAccountHandler(
    IAccountStorageService storage,
    IClientVerificationService clientVerification,
    ICurrencyService currencyService) : IRequestHandler<UpdateAccountCommand, Result<Account>>
{
    public Task<Result<Account>> Handle(UpdateAccountCommand request, CancellationToken token)
    {
        var account = storage.GetAccount(request.Id);
        if (account == null) return Task.FromResult(Result<Account>.Failure("Счёт не найден", 404));
        if (request.ClosingDate.HasValue && request.ClosingDate.Value < account.OpeningDate)
            return Task.FromResult(Result<Account>.Failure("Дата закрытия должна быть позже даты открытия"));
        if (!clientVerification.VerifyClientExists(request.OwnerId))
            return Task.FromResult(Result<Account>.Failure("Клиент не найден", 404));
        if (!currencyService.IsCurrencySupported(request.Currency))
            return Task.FromResult(Result<Account>.Failure("Валюта не поддерживается"));
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            return Task.FromResult(Result<Account>.Failure("Необходима процентная ставка"));
        account.OwnerId = request.OwnerId;
        account.Type = request.Type;
        account.Currency = request.Currency;
        account.Balance = request.Balance;
        account.InterestRate = request.InterestRate;
        account.ClosingDate = request.ClosingDate;
        var updatedAccount = storage.UpdateAccount(account);
        return Task.FromResult(Result<Account>.Success(updatedAccount!));
    }
}