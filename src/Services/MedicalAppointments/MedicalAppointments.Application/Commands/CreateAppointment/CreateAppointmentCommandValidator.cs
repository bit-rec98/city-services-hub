using FluentValidation;

namespace MedicalAppointments.Application.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required");

        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required")
            .MaximumLength(200).WithMessage("Patient name cannot exceed 200 characters");

        RuleFor(x => x.PatientEmail)
            .NotEmpty().WithMessage("Patient email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required");

        RuleFor(x => x.AppointmentDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future");
    }
}
