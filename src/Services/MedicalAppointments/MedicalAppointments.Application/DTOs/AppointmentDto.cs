namespace MedicalAppointments.Application.DTOs;

public record AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string PatientEmail { get; init; } = string.Empty;
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public string Specialty { get; init; } = string.Empty;
    public DateTime AppointmentDate { get; init; }
    public TimeSpan Duration { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
