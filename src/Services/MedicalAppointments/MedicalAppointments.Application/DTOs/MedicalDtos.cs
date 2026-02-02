using MedicalAppointments.Domain.Enums;

namespace MedicalAppointments.Application.DTOs;

/// <summary>
/// DTO para centro de salud.
/// </summary>
public record HealthCenterDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Province { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsActive { get; init; }
    public IEnumerable<string> Specialties { get; init; } = Enumerable.Empty<string>();
}

/// <summary>
/// DTO para crear un centro de salud.
/// </summary>
public record CreateHealthCenterDto
{
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Province { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}

/// <summary>
/// DTO para médico.
/// </summary>
public record DoctorDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string LicenseNumber { get; init; } = null!;
    public string PrimarySpecialty { get; init; } = null!;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public int DefaultAppointmentDuration { get; init; }
    public bool IsActive { get; init; }
    public IEnumerable<string> SecondarySpecialties { get; init; } = Enumerable.Empty<string>();
}

/// <summary>
/// DTO para turno médico.
/// </summary>
public record AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = null!;
    public string PatientDocumentNumber { get; init; } = null!;
    public string? PatientPhoneNumber { get; init; }
    public string? PatientEmail { get; init; }
    
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; } = null!;
    public string Specialty { get; init; } = null!;
    
    public Guid HealthCenterId { get; init; }
    public string HealthCenterName { get; init; } = null!;
    
    public DateTime ScheduledDate { get; init; }
    public string StartTime { get; init; } = null!;
    public string EndTime { get; init; } = null!;
    public int DurationMinutes { get; init; }
    
    public string Status { get; init; } = null!;
    public string AttentionType { get; init; } = null!;
    
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public string? CancellationReason { get; init; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// DTO para crear un turno.
/// </summary>
public record CreateAppointmentDto
{
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = null!;
    public string PatientDocumentNumber { get; init; } = null!;
    public string? PatientPhoneNumber { get; init; }
    public string? PatientEmail { get; init; }
    
    public Guid DoctorId { get; init; }
    public Guid HealthCenterId { get; init; }
    public MedicalSpecialty Specialty { get; init; }
    
    public DateTime ScheduledDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public int? DurationMinutes { get; init; }
    
    public AttentionType AttentionType { get; init; } = AttentionType.InPerson;
    public string? Reason { get; init; }
}

/// <summary>
/// DTO para reprogramar un turno.
/// </summary>
public record RescheduleAppointmentDto
{
    public DateTime NewDate { get; init; }
    public TimeOnly NewStartTime { get; init; }
}

/// <summary>
/// DTO para slots de tiempo disponibles.
/// </summary>
public record AvailableSlotDto
{
    public DateTime Date { get; init; }
    public string StartTime { get; init; } = null!;
    public string EndTime { get; init; } = null!;
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; } = null!;
    public Guid HealthCenterId { get; init; }
    public string HealthCenterName { get; init; } = null!;
}
