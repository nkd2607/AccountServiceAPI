using AccountService.Models;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<Account?>;

public class GetAccountByIdHandler(IAccountStorageService storage) : IRequestHandler<GetAccountByIdQuery, Account?>
{
    public Task<Account?> Handle(GetAccountByIdQuery query, CancellationToken token) => Task.FromResult(storage.GetAccount(query.Id));
}