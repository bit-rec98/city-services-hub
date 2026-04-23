namespace CityServicesHub.BuildingBlocks.Common.Application;

/// <summary>
/// Clase que encapsula el resultado de una operación.
/// Implementa el patrón Result para manejo de errores sin excepciones.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();
        
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
    
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Resultado genérico con valor de retorno.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}

/// <summary>
/// Representa un error con código y mensaje.
/// </summary>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "El valor proporcionado es nulo.");
    public static readonly Error NotFound = new("Error.NotFound", "El recurso solicitado no fue encontrado.");
    public static readonly Error Validation = new("Error.Validation", "Se produjo un error de validación.");
    public static readonly Error Conflict = new("Error.Conflict", "El recurso ya existe.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "No tiene autorización para realizar esta acción.");
    public static readonly Error Forbidden = new("Error.Forbidden", "Acceso denegado.");
}
