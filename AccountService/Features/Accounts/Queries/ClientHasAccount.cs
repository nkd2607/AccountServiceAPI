using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Queries;

public record ClientHasAccountQuery(Guid ClientId) : IRequest<bool>;

public class ClientHasAccountHandler(IAccountStorageService storage) : IRequestHandler<ClientHasAccountQuery, bool>
{
    public Task<bool> Handle(ClientHasAccountQuery query, CancellationToken token) => Task.FromResult(storage.GetAccounts().Any(a => a.OwnerId == query.ClientId));
}