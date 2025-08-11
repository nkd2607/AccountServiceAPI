using MediatR;
namespace AccountService.Features.Accounts.AccrueInterest;
public record AccrueInterestCommand(Guid AccountId) : IRequest;