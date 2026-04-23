using FluentValidation;
using Notifications.Application.Commands;
using Notifications.Domain.Entities;

namespace Notifications.Application.Validators;

/// <summary>
/// Validador para el comando de envío de notificaciones.
/// </summary>
public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El UserId es requerido.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido.")
            .MaximumLength(200).WithMessage("El título no puede superar los 200 caracteres.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es requerido.")
            .MaximumLength(2000).WithMessage("El contenido no puede superar los 2000 caracteres.");

        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("El destinatario es requerido para este canal.")
            .When(x => x.Channel == NotificationChannel.Email ||
                        x.Channel == NotificationChannel.SMS ||
                        x.Channel == NotificationChannel.WhatsApp);

        RuleFor(x => x.ScheduledFor)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha programada debe ser futura.")
            .When(x => x.ScheduledFor.HasValue);
    }
}
