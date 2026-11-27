using CityServicesHub.BuildingBlocks.Common.Application;
using FluentValidation;
using MediatR;

namespace MedicalAppointments.Application.Behaviors;

/// <summary>
/// Ejecuta los validadores de FluentValidation antes del handler. Los fallos se devuelven como
/// un <see cref="Result"/> fallido (código "Validation.Failed"), sin lanzar excepciones, para
/// que los controladores respondan 400 igual que con cualquier otro error de negocio.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public const string ErrorCode = "Validation.Failed";

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
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var messages = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .Distinct()
            .ToList();

        if (messages.Count == 0)
            return await next();

        return CreateFailure(new Error(ErrorCode, string.Join(" ", messages)));
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)Result.Failure(error);

        // TResponse es Result<TValue>: construir el fallo tipado con el método genérico Failure<TValue>.
        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failure = typeof(Result)
            .GetMethods()
            .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(valueType)
            .Invoke(null, new object[] { error })!;

        return (TResponse)failure;
    }
}
