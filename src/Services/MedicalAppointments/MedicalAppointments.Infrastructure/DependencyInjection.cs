using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddDbContext<MedicalAppointmentsDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("MedicalAppointmentsDb"),
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
