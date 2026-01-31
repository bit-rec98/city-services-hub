using CityServices.Shared.Common;
using CityServices.Shared.Events;
using MediatR;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Application.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IEventBus _eventBus;

    public CreateAppointmentCommandHandler(
        IRepository<Appointment> appointmentRepository,
        IRepository<Doctor> doctorRepository,
        IEventBus eventBus)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _eventBus = eventBus;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Validate doctor exists
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
        {
            return Result.Failure<AppointmentDto>("Doctor not found");
        }

        if (!doctor.IsAvailable)
        {
            return Result.Failure<AppointmentDto>("Doctor is not available");
        }

        // Create appointment
        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            PatientName = request.PatientName,
            PatientEmail = request.PatientEmail,
            DoctorId = request.DoctorId,
            DoctorName = doctor.FullName,
            Specialty = doctor.Specialty,
            AppointmentDate = request.AppointmentDate,
            Duration = TimeSpan.FromMinutes(30),
            Status = AppointmentStatus.Scheduled,
            Notes = request.Notes
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        // Publish domain event
        var appointmentCreatedEvent = new AppointmentCreatedEvent
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate,
            Specialty = appointment.Specialty
        };

        await _eventBus.PublishAsync(appointmentCreatedEvent, cancellationToken);

        // Map to DTO
        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientEmail = appointment.PatientEmail,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.DoctorName,
            Specialty = appointment.Specialty,
            AppointmentDate = appointment.AppointmentDate,
            Duration = appointment.Duration,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };

        return Result.Success(dto);
    }
}
