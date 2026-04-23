using Microsoft.Extensions.Logging;
using Notifications.Application.Services;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Services;

/// <summary>
/// Orquestador central de despacho de notificaciones.
/// Selecciona el canal adecuado y delega al sender correspondiente.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushNotificationSender pushSender,
        ILogger<NotificationDispatcher> logger)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _logger = logger;
    }

    public async Task DispatchAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        bool success;

        try
        {
            success = notification.Channel switch
            {
                NotificationChannel.Email => await DispatchEmailAsync(notification, cancellationToken),
                NotificationChannel.SMS => await DispatchSmsAsync(notification, cancellationToken),
                NotificationChannel.Push => await DispatchPushAsync(notification, cancellationToken),
                NotificationChannel.InApp => DispatchInApp(notification),
                NotificationChannel.WhatsApp => await DispatchSmsAsync(notification, cancellationToken), // fallback
                _ => throw new NotSupportedException($"Canal {notification.Channel} no soportado.")
            };

            if (success)
                notification.MarkAsSent();
            else
                notification.MarkAsFailed($"El canal {notification.Channel} reportó un fallo en el envío.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al despachar notificación {NotificationId} por canal {Channel}",
                notification.Id, notification.Channel);
            notification.MarkAsFailed(ex.Message);
        }
    }

    private async Task<bool> DispatchEmailAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            _logger.LogWarning(
                "Notificación {NotificationId} no tiene destinatario de email.", notification.Id);
            return false;
        }

        return await _emailSender.SendEmailAsync(
            notification.Recipient,
            notification.Title,
            notification.Content,
            cancellationToken);
    }

    private async Task<bool> DispatchSmsAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            _logger.LogWarning(
                "Notificación {NotificationId} no tiene número de teléfono.", notification.Id);
            return false;
        }

        return await _smsSender.SendSmsAsync(
            notification.Recipient,
            notification.Content,
            cancellationToken);
    }

    private async Task<bool> DispatchPushAsync(Notification notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            _logger.LogWarning(
                "Notificación {NotificationId} no tiene device token.", notification.Id);
            return false;
        }

        return await _pushSender.SendPushAsync(
            notification.Recipient,
            notification.Title,
            notification.Content,
            notification.Metadata,
            cancellationToken);
    }

    private static bool DispatchInApp(Notification notification)
    {
        // In-app notifications are persisted and consumed by the polling endpoint.
        // No external transport needed; always succeeds.
        return true;
    }
}
