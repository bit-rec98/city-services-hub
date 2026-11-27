using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CityServicesHub.BuildingBlocks.Security;

/// <summary>
/// CORS basado en una lista explícita de orígenes (Cors:AllowedOrigins). Sin orígenes
/// configurados no se permite ningún origen cruzado.
/// </summary>
public static class CorsExtensions
{
    public const string PolicyName = "AllowedOrigins";

    public static IServiceCollection AddCityServicesCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        if (origins.Any(o => o.Trim() == "*"))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins no admite el comodín '*'.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(origins)
                      .WithMethods("GET", "POST", "PUT", "DELETE")
                      .WithHeaders("Authorization", "Content-Type");
            });
        });

        return services;
    }
}
