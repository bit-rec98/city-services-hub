using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddDbContext<NotificationsDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("NotificationsDb"),
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
