using FluentValidation;
using MediatR;
using CityServicesHub.BuildingBlocks.Common.Application;

namespace Notifications.Application.Behaviors;

/// <summary>
/// Pipeline behavior que ejecuta validación FluentValidation antes de cada handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));

        // Return a failed Result if TResponse is Result or Result<T>
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(new Error("Validation.Failed", errorMessage));

        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == "Failure" && m.IsGenericMethod)
                .MakeGenericMethod(innerType);
            return (TResponse)failureMethod.Invoke(null, [new Error("Validation.Failed", errorMessage)])!;
        }

        throw new ValidationException(failures);
    }
}
