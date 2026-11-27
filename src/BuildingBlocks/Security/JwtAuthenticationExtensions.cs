using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CityServicesHub.BuildingBlocks.Security;

/// <summary>
/// Validación de JWT compartida por todos los servicios.
/// Contrato de configuración: JwtSettings:SecretKey, JwtSettings:Issuer, JwtSettings:Audience.
/// </summary>
public static class JwtAuthenticationExtensions
{
    public const string SectionName = "JwtSettings";

    /// <summary>Longitud mínima de la clave HMAC-SHA256 en bytes (256 bits).</summary>
    public const int MinimumSecretKeyBytes = 32;

    /// <summary>
    /// Registra la autenticación JWT Bearer. Falla al iniciar si falta la clave, el emisor o la
    /// audiencia, o si la clave es demasiado corta. Solo se acepta HS256.
    /// </summary>
    public static IServiceCollection AddCityServicesJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var secretKey = Require(section, "SecretKey");
        var issuer = Require(section, "Issuer");
        var audience = Require(section, "Audience");

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        if (keyBytes.Length < MinimumSecretKeyBytes)
        {
            throw new InvalidOperationException(
                $"{SectionName}:SecretKey debe tener al menos {MinimumSecretKeyBytes} bytes.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string Require(IConfigurationSection section, string key)
    {
        var value = section[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{SectionName}:{key} no está configurado.");
        }

        return value;
    }
}
