using System.Net;
using System.Text.Json;
using CityServicesHub.BuildingBlocks.Common.Exceptions;

namespace Identity.API.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Error de validación";
                errorResponse.Errors = validationEx.Errors;
                _logger.LogWarning(exception, "Validation error occurred");
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundEx.Message;
                _logger.LogWarning(exception, "Resource not found");
                break;

            case UnauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "No autorizado";
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.Message = "Acceso denegado";
                _logger.LogWarning(exception, "Forbidden access attempt");
                break;

            case ConflictException conflictEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Message = conflictEx.Message;
                _logger.LogWarning(exception, "Conflict error occurred");
                break;

            case BusinessException businessEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse.Message = businessEx.Message;
                errorResponse.Code = businessEx.Code;
                _logger.LogWarning(exception, "Business error occurred: {Code}", businessEx.Code);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ha ocurrido un error interno. Por favor, intente nuevamente.";
                _logger.LogError(exception, "An unhandled exception occurred");
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

/// <summary>
/// Modelo de respuesta de error.
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}
