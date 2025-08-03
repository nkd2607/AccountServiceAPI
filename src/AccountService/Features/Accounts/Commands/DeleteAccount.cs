using AccountService.Results;
using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record DeleteAccountCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteAccountHandler(IAccountStorageService storage) : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken token)
    {
        var success = storage.DeleteAccount(request.Id);
        return success
            ? Task.FromResult(Result<bool>.Success(true))
            : Task.FromResult(Result<bool>.Failure("Счёт не найден", 404));
    }
}