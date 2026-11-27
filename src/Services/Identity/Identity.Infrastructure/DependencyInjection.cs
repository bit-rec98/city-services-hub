using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Identity.Application.Interfaces;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;

namespace Identity.Infrastructure;

/// <summary>
/// Extensión para registrar servicios de infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // PostgreSQL con EF Core
        // Las colecciones y diccionarios se guardan como jsonb: Npgsql 8 exige habilitar
        // explícitamente la serialización JSON dinámica en el data source.
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("IdentityDb")
                ?? throw new InvalidOperationException("ConnectionStrings:IdentityDb no está configurado.");

            return new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson().Build();
        });

        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
        });

        // Redis para caché distribuido
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Identity_";
        });

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();

        // Servicios
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();

        return services;
    }
}
