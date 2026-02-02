namespace CityServicesHub.BuildingBlocks.Common.Exceptions;

/// <summary>
/// Excepción para errores de validación.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Se han producido uno o más errores de validación.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string error)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { error } }
        };
    }
}

/// <summary>
/// Excepción cuando no se encuentra un recurso.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base("El recurso solicitado no fue encontrado.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"La entidad \"{name}\" ({key}) no fue encontrada.")
    {
    }
}

/// <summary>
/// Excepción para errores de negocio.
/// </summary>
public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string message)
        : base(message)
    {
        Code = "BUSINESS_ERROR";
    }

    public BusinessException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// Excepción para conflictos (recurso ya existe).
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base("El recurso ya existe.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Excepción para acceso no autorizado.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("No tiene autorización para acceder a este recurso.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Excepción para acceso prohibido.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("Acceso denegado.")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}
