using AccountService.Results;
using FluentValidation;
using MediatR;

namespace AccountService.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    where TResponse : Result<TResponse>, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(result => result.Errors).Where(f => f != null).ToList();
        if (failures.Count == 0) return await next(cancellationToken);
        if (!typeof(TResponse).IsGenericType || typeof(TResponse).GetGenericTypeDefinition() != typeof(Result<>))
            throw new ValidationException(failures);
        var errorMessage = failures.First().ErrorMessage;
        return (TResponse)Result<TResponse>.Failure(errorMessage);
    }
}