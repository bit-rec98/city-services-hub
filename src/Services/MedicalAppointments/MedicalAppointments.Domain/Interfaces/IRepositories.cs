using CityServicesHub.BuildingBlocks.Common.Interfaces;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Enums;

namespace MedicalAppointments.Domain.Interfaces;

/// <summary>
/// Repositorio de centros de salud.
/// </summary>
public interface IHealthCenterRepository : IRepository<HealthCenter>
{
    Task<HealthCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<HealthCenter>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<HealthCenter>> GetBySpecialtyAsync(MedicalSpecialty specialty, CancellationToken cancellationToken = default);
    Task<IEnumerable<HealthCenter>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositorio de médicos.
/// </summary>
public interface IDoctorRepository : IRepository<Doctor>
{
    Task<Doctor?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(MedicalSpecialty specialty, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetByHealthCenterAsync(Guid healthCenterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositorio de turnos médicos.
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDoctorAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByHealthCenterAsync(Guid healthCenterId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetUpcomingByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetPendingRemindersAsync(DateTime reminderDate, CancellationToken cancellationToken = default);
    Task<bool> HasConflictAsync(Guid doctorId, DateTime date, TimeOnly startTime, TimeOnly endTime, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
