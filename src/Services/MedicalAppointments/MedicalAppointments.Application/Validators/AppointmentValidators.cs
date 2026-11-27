using FluentValidation;
using MedicalAppointments.Application.Commands;

namespace MedicalAppointments.Application.Validators;

/// <summary>
/// Validación de entrada para crear un turno. Los largos máximos coinciden con las columnas de la base.
/// El paciente no se valida aquí: lo determina el handler a partir del usuario autenticado.
/// </summary>
public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public const int MaxDurationMinutes = 480;

    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("El nombre del paciente es requerido.")
            .MaximumLength(200).WithMessage("El nombre del paciente no puede exceder los 200 caracteres.");

        RuleFor(x => x.PatientDocumentNumber)
            .NotEmpty().WithMessage("El número de documento del paciente es requerido.")
            .MaximumLength(20).WithMessage("El número de documento no puede exceder los 20 caracteres.");

        RuleFor(x => x.PatientPhoneNumber)
            .MaximumLength(50).WithMessage("El teléfono no puede exceder los 50 caracteres.");

        RuleFor(x => x.PatientEmail)
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .MaximumLength(256).WithMessage("El email no puede exceder los 256 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.PatientEmail));

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El médico es requerido.");

        RuleFor(x => x.HealthCenterId)
            .NotEmpty().WithMessage("El centro de salud es requerido.");

        RuleFor(x => x.Specialty)
            .IsInEnum().WithMessage("La especialidad no es válida.");

        RuleFor(x => x.AttentionType)
            .IsInEnum().WithMessage("El tipo de atención no es válido.");

        RuleFor(x => x.ScheduledDate)
            .Must(date => date.Date >= DateTime.Today)
            .WithMessage("La fecha del turno no puede ser pasada.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, MaxDurationMinutes)
            .WithMessage($"La duración debe estar entre 1 y {MaxDurationMinutes} minutos.")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("El motivo no puede exceder los 1000 caracteres.");
    }
}

/// <summary>
/// Validación de entrada para cancelar un turno.
/// </summary>
public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("El turno es requerido.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo de la cancelación es requerido.")
            .MaximumLength(500).WithMessage("El motivo de la cancelación no puede exceder los 500 caracteres.");
    }
}
