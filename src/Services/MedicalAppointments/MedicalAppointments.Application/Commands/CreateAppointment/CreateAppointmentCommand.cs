using CityServices.Shared.Common;
using MediatR;
using MedicalAppointments.Application.DTOs;

namespace MedicalAppointments.Application.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string PatientEmail { get; init; } = string.Empty;
    public Guid DoctorId { get; init; }
    public DateTime AppointmentDate { get; init; }
    public string Notes { get; init; } = string.Empty;
}
