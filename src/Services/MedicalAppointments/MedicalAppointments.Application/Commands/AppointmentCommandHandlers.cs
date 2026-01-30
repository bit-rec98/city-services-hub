using CityServicesHub.BuildingBlocks.Common.Application;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Interfaces;

namespace MedicalAppointments.Application.Commands;

/// <summary>
/// Handler para crear un turno médico.
/// </summary>
public class CreateAppointmentCommandHandler : ICommandHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IHealthCenterRepository _healthCenterRepository;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IHealthCenterRepository healthCenterRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _healthCenterRepository = healthCenterRepository;
    }

    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand request, 
        CancellationToken cancellationToken)
    {
        // Obtener médico
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor is null)
            return Result.Failure<AppointmentDto>(new Error("Doctor.NotFound", "El médico no fue encontrado."));

        // Obtener centro de salud
        var healthCenter = await _healthCenterRepository.GetByIdAsync(request.HealthCenterId, cancellationToken);
        if (healthCenter is null)
            return Result.Failure<AppointmentDto>(new Error("HealthCenter.NotFound", "El centro de salud no fue encontrado."));

        // Calcular duración
        var duration = request.DurationMinutes ?? doctor.DefaultAppointmentDuration;
        var endTime = request.StartTime.AddMinutes(duration);

        // Verificar conflictos
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            request.DoctorId,
            request.ScheduledDate,
            request.StartTime,
            endTime,
            cancellationToken: cancellationToken);

        if (hasConflict)
            return Result.Failure<AppointmentDto>(new Error(
                "Appointment.Conflict", 
                "Ya existe un turno agendado en ese horario para el médico seleccionado."));

        // Crear turno
        var appointment = Appointment.Create(
            request.PatientId,
            request.PatientName,
            request.PatientDocumentNumber,
            request.DoctorId,
            doctor.FullName,
            request.Specialty,
            request.HealthCenterId,
            healthCenter.Name,
            request.ScheduledDate,
            request.StartTime,
            duration,
            request.AttentionType,
            request.Reason);

        appointment.SetPatientContact(request.PatientPhoneNumber, request.PatientEmail);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        // Mapear a DTO
        var dto = MapToDto(appointment);
        return Result.Success(dto);
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientDocumentNumber = appointment.PatientDocumentNumber,
            PatientPhoneNumber = appointment.PatientPhoneNumber,
            PatientEmail = appointment.PatientEmail,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.DoctorName,
            Specialty = appointment.Specialty.ToString(),
            HealthCenterId = appointment.HealthCenterId,
            HealthCenterName = appointment.HealthCenterName,
            ScheduledDate = appointment.ScheduledDate,
            StartTime = appointment.StartTime.ToString("HH:mm"),
            EndTime = appointment.EndTime.ToString("HH:mm"),
            DurationMinutes = appointment.DurationMinutes,
            Status = appointment.Status.ToString(),
            AttentionType = appointment.AttentionType.ToString(),
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            ConfirmedAt = appointment.ConfirmedAt,
            CancelledAt = appointment.CancelledAt,
            CompletedAt = appointment.CompletedAt
        };
    }
}

/// <summary>
/// Handler para cancelar un turno.
/// </summary>
public class CancelAppointmentCommandHandler : ICommandHandler<CancelAppointmentCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        
        if (appointment is null)
            return Result.Failure(new Error("Appointment.NotFound", "El turno no fue encontrado."));

        if (!appointment.CanBeCancelled)
            return Result.Failure(new Error("Appointment.CannotCancel", "El turno no puede ser cancelado."));

        appointment.Cancel(request.Reason);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Handler para confirmar un turno.
/// </summary>
public class ConfirmAppointmentCommandHandler : ICommandHandler<ConfirmAppointmentCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public ConfirmAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        
        if (appointment is null)
            return Result.Failure(new Error("Appointment.NotFound", "El turno no fue encontrado."));

        try
        {
            appointment.Confirm();
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Appointment.InvalidOperation", ex.Message));
        }
    }
}
