using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using MedicalAppointments.Domain.Interfaces;
using MedicalAppointments.Infrastructure.Data;
using MedicalAppointments.Infrastructure.Repositories;

namespace MedicalAppointments.Infrastructure;

/// <summary>
/// Extensión para registrar servicios de infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMedicalAppointmentsInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // PostgreSQL con EF Core
        // Las colecciones y diccionarios se guardan como jsonb: Npgsql 8 exige habilitar
        // explícitamente la serialización JSON dinámica en el data source.
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("MedicalAppointmentsDb")
                ?? throw new InvalidOperationException("ConnectionStrings:MedicalAppointmentsDb no está configurado.");

            return new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson().Build();
        });

        services.AddDbContext<MedicalAppointmentsDbContext>((serviceProvider, options) =>
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
            options.InstanceName = "MedicalAppointments_";
        });

        // Repositorios
        services.AddScoped<IHealthCenterRepository, HealthCenterRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
