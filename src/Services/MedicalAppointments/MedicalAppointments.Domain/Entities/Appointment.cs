using CityServices.Shared.Common;

namespace MedicalAppointments.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan Duration { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Cancelled,
    Completed,
    NoShow
}
