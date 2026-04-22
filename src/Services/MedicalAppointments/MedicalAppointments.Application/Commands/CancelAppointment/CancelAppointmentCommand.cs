using CityServices.Shared.Common;
using MediatR;

namespace MedicalAppointments.Application.Commands.CancelAppointment;

public record CancelAppointmentCommand : IRequest<Result<bool>>
{
    public Guid AppointmentId { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
}
