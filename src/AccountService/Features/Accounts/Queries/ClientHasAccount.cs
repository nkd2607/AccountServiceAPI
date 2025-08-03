using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Queries;

public record ClientHasAccountQuery(Guid ClientId) : IRequest<Result<bool>>;

public class ClientHasAccountHandler(IAccountStorageService storage)
    : IRequestHandler<ClientHasAccountQuery, Result<bool>>
{
    public Task<Result<bool>> Handle(ClientHasAccountQuery query, CancellationToken token)
    {
        var hasAccount = storage.GetAccounts().Any(a => a.OwnerId == query.ClientId);
        return Task.FromResult(Result<bool>.Success(hasAccount));
    }
}