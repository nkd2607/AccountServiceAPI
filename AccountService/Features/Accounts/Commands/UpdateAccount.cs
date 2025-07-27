using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record UpdateAccountCommand(Guid Id, Guid OwnerId, AccountType Type, string Currency, decimal Balance, decimal? InterestRate, DateTime? ClosingDate) : IRequest<Account?>;

public class UpdateAccountHandler(IAccountStorageService storage, IClientVerificationService clientVerification, ICurrencyService currencyService) : IRequestHandler<UpdateAccountCommand, Account?>
{
    public Task<Account?> Handle(UpdateAccountCommand request, CancellationToken token)
    {
        var account = storage.GetAccount(request.Id);
        if (account == null) return Task.FromResult<Account?>(null);

        if (request.ClosingDate.HasValue && request.ClosingDate.Value < account.OpeningDate)
            throw new Exception("Дата закрытия не может быть раньше даты открытия");

        if (!clientVerification.VerifyClientExists(request.OwnerId))
            throw new Exception("Клиент не найден");

        if (!currencyService.IsCurrencySupported(request.Currency))
            throw new Exception("Валюта не поддерживается");

        if (request.Type != AccountType.Checking && request.InterestRate == null)
            throw new Exception("Для открытия вклада/кредита требуется процентная ставка");

        account.OwnerId = request.OwnerId;
        account.Type = request.Type;
        account.Currency = request.Currency;
        account.Balance = request.Balance;
        account.InterestRate = request.InterestRate;
        account.ClosingDate = request.ClosingDate;

        return Task.FromResult(storage.UpdateAccount(account));
    }
}