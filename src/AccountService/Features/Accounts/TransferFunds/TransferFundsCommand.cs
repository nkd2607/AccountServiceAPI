using MediatR;

namespace AccountService.Features.Accounts.TransferFunds;

public record TransferFundsCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount) : IRequest;