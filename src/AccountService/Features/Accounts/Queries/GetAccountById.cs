using AccountService.Models;
using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<Result<Account>>;

public class GetAccountByIdHandler(IAccountStorageService storage)
    : IRequestHandler<GetAccountByIdQuery, Result<Account>>
{
    public Task<Result<Account>> Handle(GetAccountByIdQuery query, CancellationToken token)
    {
        var account = storage.GetAccount(query.Id);
        return account == null
            ? Task.FromResult(Result<Account>.Failure("Счёт не найден", 404))
            : Task.FromResult(Result<Account>.Success(account));
    }
}