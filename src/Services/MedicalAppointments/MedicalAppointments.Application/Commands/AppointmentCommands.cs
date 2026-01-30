using CityServicesHub.BuildingBlocks.Common.Application;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Enums;

namespace MedicalAppointments.Application.Commands;

/// <summary>
/// Comando para crear un turno médico.
/// </summary>
public record CreateAppointmentCommand(
    Guid PatientId,
    string PatientName,
    string PatientDocumentNumber,
    string? PatientPhoneNumber,
    string? PatientEmail,
    Guid DoctorId,
    Guid HealthCenterId,
    MedicalSpecialty Specialty,
    DateTime ScheduledDate,
    TimeOnly StartTime,
    int? DurationMinutes,
    AttentionType AttentionType,
    string? Reason
) : ICommand<AppointmentDto>;

/// <summary>
/// Comando para cancelar un turno.
/// </summary>
public record CancelAppointmentCommand(
    Guid AppointmentId,
    string Reason
) : ICommand;

/// <summary>
/// Comando para confirmar un turno.
/// </summary>
public record ConfirmAppointmentCommand(Guid AppointmentId) : ICommand;

/// <summary>
/// Comando para reprogramar un turno.
/// </summary>
public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTime NewDate,
    TimeOnly NewStartTime
) : ICommand<AppointmentDto>;

/// <summary>
/// Comando para completar un turno.
/// </summary>
public record CompleteAppointmentCommand(
    Guid AppointmentId,
    string? Notes
) : ICommand;

/// <summary>
/// Comando para marcar como ausente.
/// </summary>
public record MarkNoShowCommand(Guid AppointmentId) : ICommand;
