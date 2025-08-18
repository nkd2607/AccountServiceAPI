using AccountService.Models;
using AccountService.Results;
using MediatR;
namespace AccountService.Features.Accounts.AccrueInterest;
public record AccrueInterestCommand(Guid AccountId, DateTime PeriodFrom, DateTime PeriodTo, decimal Amount) : IRequest<Result<Account>>;