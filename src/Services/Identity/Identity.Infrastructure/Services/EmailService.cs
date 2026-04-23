using Microsoft.Extensions.Logging;
using Identity.Application.Interfaces;

namespace Identity.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de email (stub para desarrollo).
/// En producción se integraría con SendGrid, Amazon SES, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        // En producción: integrar con SendGrid, Amazon SES, etc.
        _logger.LogInformation(
            "Email de verificación enviado a {Email} con token {Token}", 
            email, 
            token[..Math.Min(10, token.Length)] + "...");
        
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email de reset de contraseña enviado a {Email} con token {Token}", 
            email, 
            token[..Math.Min(10, token.Length)] + "...");
        
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email de bienvenida enviado a {Email} para {FirstName}", 
            email, 
            firstName);
        
        return Task.CompletedTask;
    }
}
