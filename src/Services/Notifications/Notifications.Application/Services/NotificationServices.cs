using Notifications.Domain.Entities;

namespace Notifications.Application.Services;

/// <summary>
/// Servicio para envío de notificaciones.
/// </summary>
public interface INotificationSender
{
    Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio para envío de emails.
/// </summary>
public interface IEmailSender
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio para envío de SMS.
/// </summary>
public interface ISmsSender
{
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio para notificaciones push.
/// </summary>
public interface IPushNotificationSender
{
    Task<bool> SendPushAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de templates de notificaciones.
/// </summary>
public interface INotificationTemplateService
{
    Task<(string Subject, string Body)> RenderTemplateAsync(string templateId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}

/// <summary>
/// Despachador de notificaciones. Selecciona el canal correcto y despacha la notificación.
/// </summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(Notification notification, CancellationToken cancellationToken = default);
}
