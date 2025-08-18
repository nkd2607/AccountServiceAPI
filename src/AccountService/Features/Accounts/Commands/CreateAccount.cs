using AccountService.Domain.Events;        
using AccountService.Infrastructure.Outbox;
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
    ICurrencyService currencyService,
    OutboxService outbox)
    : IRequestHandler<CreateAccountCommand, Result<Account>>
{
    public async Task<Result<Account>> Handle(CreateAccountCommand request, CancellationToken token)
    {
        if (!clientVerification.VerifyClientExists(request.OwnerId))
            return Result<Account>.Failure("Счёт не найден", 404);

        if (!currencyService.IsCurrencySupported(request.Currency))
            return Result<Account>.Failure("Валюта не поддерживается");

        if (request.Type != AccountType.Checking && request.InterestRate == null)
            return Result<Account>.Failure("Необходима процентная ставка");

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

        var evt = new AccountOpened(
            Guid.NewGuid(),          
            DateTime.UtcNow,           
            createdAccount.Id,         
            createdAccount.OwnerId,    
            createdAccount.Currency,   
            createdAccount.Type.ToString() 
        );

        await outbox.AddAsync(evt, token);

        return Result<Account>.Success(createdAccount, 201);
    }
}