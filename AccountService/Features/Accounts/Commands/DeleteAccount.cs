using AccountService.Services.Interfaces;
using MediatR;

namespace AccountService.Features.Accounts.Commands;

public record DeleteAccountCommand(Guid Id) : IRequest<bool>;

public class DeleteAccountHandler(IAccountStorageService storage) : IRequestHandler<DeleteAccountCommand, bool>
{
    public Task<bool> Handle(DeleteAccountCommand request, CancellationToken token) => Task.FromResult(storage.DeleteAccount(request.Id));
}