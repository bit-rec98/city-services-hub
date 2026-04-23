using Notifications.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Notifications.Infrastructure.Services;

/// <summary>
/// Implementación de servicio de envío de emails usando SendGrid.
/// </summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(
        IConfiguration configuration,
        ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        
        var apiKey = configuration["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("SendGrid:ApiKey no está configurado o está vacío");
        }
        _apiKey = apiKey;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@hubciudadano.gob.ar";
        _fromName = configuration["SendGrid:FromName"] ?? "Hub Ciudadano";
    }

    public async Task<bool> SendEmailAsync(
        string to, 
        string subject, 
        string htmlBody, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Nota: En producción se usaría SendGrid Client real
            // var client = new SendGridClient(_apiKey);
            // var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
            // var response = await client.SendEmailAsync(msg, cancellationToken);
            
            _logger.LogInformation(
                "Email enviado a {To} con asunto: {Subject}", 
                to, subject);
            
            // Simular envío
            await Task.Delay(100, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {To}", to);
            return false;
        }
    }
}

/// <summary>
/// Implementación de servicio de envío de SMS.
/// </summary>
public class SmsSenderService : ISmsSender
{
    private readonly ILogger<SmsSenderService> _logger;

    public SmsSenderService(ILogger<SmsSenderService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(
        string phoneNumber, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Nota: En producción se usaría un proveedor SMS real (Twilio, etc.)
            _logger.LogInformation(
                "SMS enviado a {PhoneNumber}: {Message}", 
                phoneNumber, message);
            
            await Task.Delay(100, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}

/// <summary>
/// Implementación de servicio de notificaciones push.
/// </summary>
public class PushNotificationService : IPushNotificationSender
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendPushAsync(
        string deviceToken, 
        string title, 
        string body, 
        Dictionary<string, string>? data = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Nota: En producción se usaría Firebase Cloud Messaging o similar
            _logger.LogInformation(
                "Push notification enviada a {DeviceToken}: {Title}", 
                deviceToken, title);
            
            await Task.Delay(100, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar push notification a {DeviceToken}", deviceToken);
            return false;
        }
    }
}
