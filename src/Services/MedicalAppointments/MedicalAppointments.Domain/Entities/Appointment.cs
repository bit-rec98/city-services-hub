using CityServicesHub.BuildingBlocks.Common.Domain;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.Events;

namespace MedicalAppointments.Domain.Entities;

/// <summary>
/// Entidad que representa un turno médico.
/// </summary>
public class Appointment : AggregateRoot
{
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = null!;
    public string PatientDocumentNumber { get; private set; } = null!;
    public string? PatientPhoneNumber { get; private set; }
    public string? PatientEmail { get; private set; }
    
    public Guid DoctorId { get; private set; }
    public string DoctorName { get; private set; } = null!;
    public MedicalSpecialty Specialty { get; private set; }
    
    public Guid HealthCenterId { get; private set; }
    public string HealthCenterName { get; private set; } = null!;
    
    public DateTime ScheduledDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }
    
    public AppointmentStatus Status { get; private set; }
    public AttentionType AttentionType { get; private set; }
    
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    public bool ReminderSent { get; private set; }
    public DateTime? ReminderSentAt { get; private set; }

    private Appointment() { }

    private Appointment(
        Guid id,
        Guid patientId,
        string patientName,
        string patientDocumentNumber,
        Guid doctorId,
        string doctorName,
        MedicalSpecialty specialty,
        Guid healthCenterId,
        string healthCenterName,
        DateTime scheduledDate,
        TimeOnly startTime,
        int durationMinutes,
        AttentionType attentionType,
        string? reason) : base(id)
    {
        PatientId = patientId;
        PatientName = patientName;
        PatientDocumentNumber = patientDocumentNumber;
        DoctorId = doctorId;
        DoctorName = doctorName;
        Specialty = specialty;
        HealthCenterId = healthCenterId;
        HealthCenterName = healthCenterName;
        ScheduledDate = scheduledDate.Date;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndTime = startTime.AddMinutes(durationMinutes);
        AttentionType = attentionType;
        Reason = reason;
        Status = AppointmentStatus.Scheduled;
        ReminderSent = false;
    }

    public static Appointment Create(
        Guid patientId,
        string patientName,
        string patientDocumentNumber,
        Guid doctorId,
        string doctorName,
        MedicalSpecialty specialty,
        Guid healthCenterId,
        string healthCenterName,
        DateTime scheduledDate,
        TimeOnly startTime,
        int durationMinutes,
        AttentionType attentionType = AttentionType.InPerson,
        string? reason = null)
    {
        if (scheduledDate.Date < DateTime.Today)
            throw new ArgumentException("No se puede agendar un turno en una fecha pasada.");

        var appointment = new Appointment(
            Guid.NewGuid(),
            patientId,
            patientName,
            patientDocumentNumber,
            doctorId,
            doctorName,
            specialty,
            healthCenterId,
            healthCenterName,
            scheduledDate,
            startTime,
            durationMinutes,
            attentionType,
            reason);

        appointment.AddDomainEvent(new AppointmentCreatedEvent(
            appointment.Id,
            patientId,
            doctorId,
            scheduledDate,
            startTime));

        return appointment;
    }

    public void SetPatientContact(string? phoneNumber, string? email)
    {
        PatientPhoneNumber = phoneNumber;
        PatientEmail = email;
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden confirmar turnos en estado Agendado.");

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        
        AddDomainEvent(new AppointmentConfirmedEvent(Id, PatientId));
    }

    public void Cancel(string reason)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.InProgress)
            throw new InvalidOperationException("No se puede cancelar un turno completado o en progreso.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        
        AddDomainEvent(new AppointmentCancelledEvent(Id, PatientId, DoctorId, reason));
    }

    public void Reschedule(DateTime newDate, TimeOnly newStartTime)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.InProgress)
            throw new InvalidOperationException("No se puede reprogramar un turno completado o en progreso.");

        if (newDate.Date < DateTime.Today)
            throw new ArgumentException("No se puede reprogramar a una fecha pasada.");

        var oldDate = ScheduledDate;
        var oldTime = StartTime;

        ScheduledDate = newDate.Date;
        StartTime = newStartTime;
        EndTime = newStartTime.AddMinutes(DurationMinutes);
        Status = AppointmentStatus.Rescheduled;
        ReminderSent = false;
        
        AddDomainEvent(new AppointmentRescheduledEvent(Id, PatientId, oldDate, oldTime, newDate, newStartTime));
    }

    public void StartAttention()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Solo se puede iniciar la atención de turnos confirmados o agendados.");

        Status = AppointmentStatus.InProgress;
    }

    public void Complete(string? notes = null)
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Solo se pueden completar turnos en progreso.");

        Status = AppointmentStatus.Completed;
        Notes = notes;
        CompletedAt = DateTime.UtcNow;
        
        AddDomainEvent(new AppointmentCompletedEvent(Id, PatientId, DoctorId));
    }

    public void MarkAsNoShow()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("No se puede marcar como ausente un turno completado o cancelado.");

        Status = AppointmentStatus.NoShow;
        
        AddDomainEvent(new AppointmentNoShowEvent(Id, PatientId, DoctorId));
    }

    public void MarkReminderSent()
    {
        ReminderSent = true;
        ReminderSentAt = DateTime.UtcNow;
    }

    public bool IsUpcoming => ScheduledDate >= DateTime.Today && 
                              Status != AppointmentStatus.Cancelled && 
                              Status != AppointmentStatus.Completed;

    public bool IsPast => ScheduledDate < DateTime.Today || 
                          Status == AppointmentStatus.Completed || 
                          Status == AppointmentStatus.NoShow;

    public bool CanBeCancelled => Status == AppointmentStatus.Scheduled || 
                                  Status == AppointmentStatus.Confirmed ||
                                  Status == AppointmentStatus.Rescheduled;
}
