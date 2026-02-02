using CityServicesHub.BuildingBlocks.Common.Domain;

namespace MedicalAppointments.Domain.Events;

/// <summary>
/// Evento: Centro de salud creado.
/// </summary>
public record HealthCenterCreatedEvent(Guid HealthCenterId, string Name, string Code) : DomainEvent
{
    public override string EventType => "HealthCenterCreated";
}

/// <summary>
/// Evento: Turno creado.
/// </summary>
public record AppointmentCreatedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledDate,
    TimeOnly StartTime) : DomainEvent
{
    public override string EventType => "AppointmentCreated";
}

/// <summary>
/// Evento: Turno confirmado.
/// </summary>
public record AppointmentConfirmedEvent(Guid AppointmentId, Guid PatientId) : DomainEvent
{
    public override string EventType => "AppointmentConfirmed";
}

/// <summary>
/// Evento: Turno cancelado.
/// </summary>
public record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    string Reason) : DomainEvent
{
    public override string EventType => "AppointmentCancelled";
}

/// <summary>
/// Evento: Turno reprogramado.
/// </summary>
public record AppointmentRescheduledEvent(
    Guid AppointmentId,
    Guid PatientId,
    DateTime OldDate,
    TimeOnly OldTime,
    DateTime NewDate,
    TimeOnly NewTime) : DomainEvent
{
    public override string EventType => "AppointmentRescheduled";
}

/// <summary>
/// Evento: Turno completado.
/// </summary>
public record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId) : DomainEvent
{
    public override string EventType => "AppointmentCompleted";
}

/// <summary>
/// Evento: Paciente no se presentó.
/// </summary>
public record AppointmentNoShowEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId) : DomainEvent
{
    public override string EventType => "AppointmentNoShow";
}
