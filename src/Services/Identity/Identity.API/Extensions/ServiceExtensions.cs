using Microsoft.OpenApi.Models;

namespace Identity.API.Extensions;

/// <summary>
/// Extensiones para configuración de servicios de la API.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Identity API",
                Version = "v1",
                Description = "API de autenticación y autorización del Hub de Servicios Ciudadanos",
                Contact = new OpenApiContact
                {
                    Name = "City Services Hub Team",
                    Email = "soporte@cityserviceshub.gob.ar"
                }
            });

            // Configuración de seguridad JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
