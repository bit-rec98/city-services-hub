using CityServices.Shared.Common;

namespace MedicalAppointments.Domain.Entities;

public class Doctor : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
