using AccountService.Models;
using AccountService.Models.Enums;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Queries;

public record GetAccountsQuery(Guid? OwnerId = null, AccountType? Type = null, string? Currency = null) : IRequest<IEnumerable<Account>>;

public class GetAccountsHandler(IAccountStorageService storage) : IRequestHandler<GetAccountsQuery, IEnumerable<Account>>
{
    public Task<IEnumerable<Account>> Handle(GetAccountsQuery query, CancellationToken token)
    {
        var accounts = storage.GetAccounts();

        if (query.OwnerId.HasValue) 
            accounts = accounts.Where(a => a.OwnerId == query.OwnerId.Value);

        if (query.Type.HasValue)
            accounts = accounts.Where(a => a.Type == query.Type.Value);

        if (!string.IsNullOrEmpty(query.Currency)) 
            accounts = accounts.Where(a => a.Currency.Equals(query.Currency, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(accounts);
    }
}