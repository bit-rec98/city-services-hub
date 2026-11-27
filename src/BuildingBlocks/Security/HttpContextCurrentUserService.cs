using System.Security.Claims;
using CityServicesHub.BuildingBlocks.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CityServicesHub.BuildingBlocks.Security;

/// <summary>
/// Expone la identidad del usuario autenticado de la petición HTTP actual. La identidad proviene
/// únicamente del token ya validado; nunca de datos enviados por el cliente en el cuerpo o la ruta.
/// </summary>
public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => IsAuthenticated
        ? Principal!.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal.FindFirstValue("sub")
        : null;

    public string? UserName => IsAuthenticated ? Principal!.Identity?.Name : null;

    public string? Email => IsAuthenticated
        ? Principal!.FindFirstValue(ClaimTypes.Email) ?? Principal.FindFirstValue("email")
        : null;

    public IEnumerable<string> Roles => IsAuthenticated
        ? Principal!.FindAll(ClaimTypes.Role).Concat(Principal.FindAll("role")).Select(c => c.Value).Distinct()
        : Enumerable.Empty<string>();

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.Ordinal);

    public bool HasClaim(string claimType, string claimValue) =>
        IsAuthenticated && Principal!.HasClaim(claimType, claimValue);
}

public static class CurrentUserExtensions
{
    /// <summary>Registra <see cref="ICurrentUserService"/> respaldado por el HttpContext.</summary>
    public static IServiceCollection AddCityServicesCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        return services;
    }
}
