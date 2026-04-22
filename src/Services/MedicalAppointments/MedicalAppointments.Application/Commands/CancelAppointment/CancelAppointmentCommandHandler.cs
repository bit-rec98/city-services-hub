using CityServices.Shared.Common;
using CityServices.Shared.Events;
using MediatR;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Application.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result<bool>>
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IEventBus _eventBus;

    public CancelAppointmentCommandHandler(
        IRepository<Appointment> appointmentRepository,
        IEventBus eventBus)
    {
        _appointmentRepository = appointmentRepository;
        _eventBus = eventBus;
    }

    public async Task<Result<bool>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);

        if (appointment == null)
        {
            return Result.Failure<bool>("Appointment not found");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result.Failure<bool>("Appointment is already cancelled");
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result.Failure<bool>("Cannot cancel a completed appointment");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.CancellationReason;

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        await _eventBus.PublishAsync(new AppointmentCancelledEvent
        {
            AppointmentId = appointment.Id,
            Reason = request.CancellationReason
        }, cancellationToken);

        return Result.Success(true);
    }
}
