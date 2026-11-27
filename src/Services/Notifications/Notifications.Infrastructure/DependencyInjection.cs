using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Notifications.Application.Interfaces;
using Notifications.Application.Services;
using Notifications.Infrastructure.Data;
using Notifications.Infrastructure.Repositories;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure;

/// <summary>
/// Registro de servicios de la capa Infrastructure.
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
            var connectionString = configuration.GetConnectionString("NotificationsDb")
                ?? throw new InvalidOperationException("ConnectionStrings:NotificationsDb no está configurado.");

            return new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson().Build();
        });

        services.AddDbContext<NotificationsDbContext>((serviceProvider, options) =>
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

        // Repositorios
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Channel senders
        services.AddScoped<IEmailSender, SendGridEmailSender>();
        services.AddScoped<ISmsSender, SmsSenderService>();
        services.AddScoped<IPushNotificationSender, PushNotificationService>();

        // Dispatcher
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}
