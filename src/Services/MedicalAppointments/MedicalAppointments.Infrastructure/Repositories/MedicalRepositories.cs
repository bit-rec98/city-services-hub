using Microsoft.EntityFrameworkCore;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.Interfaces;
using MedicalAppointments.Infrastructure.Data;
using System.Linq.Expressions;

namespace MedicalAppointments.Infrastructure.Repositories;

/// <summary>
/// Repositorio de centros de salud.
/// </summary>
public class HealthCenterRepository : IHealthCenterRepository
{
    private readonly MedicalAppointmentsDbContext _context;

    public HealthCenterRepository(MedicalAppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<HealthCenter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthCenter>> FindAsync(
        Expression<Func<HealthCenter, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<HealthCenter> AddAsync(HealthCenter entity, CancellationToken cancellationToken = default)
    {
        await _context.HealthCenters.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(HealthCenter entity, CancellationToken cancellationToken = default)
    {
        _context.HealthCenters.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(HealthCenter entity, CancellationToken cancellationToken = default)
    {
        entity.SetDeletedInfo(null);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.AnyAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<HealthCenter, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _context.HealthCenters.CountAsync(cancellationToken)
            : await _context.HealthCenters.CountAsync(predicate, cancellationToken);
    }

    public async Task<HealthCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.FirstOrDefaultAsync(h => h.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<HealthCenter>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters.Where(h => h.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthCenter>> GetBySpecialtyAsync(
        MedicalSpecialty specialty, 
        CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters
            .Where(h => h.IsActive && h.Specialties.Contains(specialty))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthCenter>> GetByCityAsync(
        string city, 
        CancellationToken cancellationToken = default)
    {
        return await _context.HealthCenters
            .Where(h => h.IsActive && h.City.ToLower() == city.ToLower())
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repositorio de médicos.
/// </summary>
public class DoctorRepository : IDoctorRepository
{
    private readonly MedicalAppointmentsDbContext _context;

    public DoctorRepository(MedicalAppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Doctors.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> FindAsync(
        Expression<Func<Doctor, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Doctors.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Doctor> AddAsync(Doctor entity, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Doctor entity, CancellationToken cancellationToken = default)
    {
        _context.Doctors.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Doctor entity, CancellationToken cancellationToken = default)
    {
        entity.SetDeletedInfo(null);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<Doctor, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _context.Doctors.CountAsync(cancellationToken)
            : await _context.Doctors.CountAsync(predicate, cancellationToken);
    }

    public async Task<Doctor?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors.FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber, cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(
        MedicalSpecialty specialty, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Where(d => d.IsActive && (d.PrimarySpecialty == specialty || d.SecondarySpecialties.Contains(specialty)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetByHealthCenterAsync(
        Guid healthCenterId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Where(d => d.IsActive && d.HealthCenterIds.Contains(healthCenterId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Doctors.Where(d => d.IsActive).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repositorio de turnos médicos.
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly MedicalAppointmentsDbContext _context;

    public AppointmentRepository(MedicalAppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> FindAsync(
        Expression<Func<Appointment, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Appointment> AddAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        entity.SetDeletedInfo(null);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<Appointment, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _context.Appointments.CountAsync(cancellationToken)
            : await _context.Appointments.CountAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(
        Guid patientId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAsync(
        Guid doctorId, 
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.ScheduledDate == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByHealthCenterAsync(
        Guid healthCenterId, 
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.HealthCenterId == healthCenterId && a.ScheduledDate == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingByPatientAsync(
        Guid patientId, 
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.Appointments
            .Where(a => a.PatientId == patientId && 
                       a.ScheduledDate >= today &&
                       a.Status != AppointmentStatus.Cancelled &&
                       a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetPendingRemindersAsync(
        DateTime reminderDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.ScheduledDate == reminderDate.Date &&
                       !a.ReminderSent &&
                       a.Status == AppointmentStatus.Scheduled)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConflictAsync(
        Guid doctorId, 
        DateTime date, 
        TimeOnly startTime, 
        TimeOnly endTime,
        Guid? excludeAppointmentId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.ScheduledDate == date.Date &&
                       a.Status != AppointmentStatus.Cancelled &&
                       ((a.StartTime < endTime && a.EndTime > startTime)));

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.ScheduledDate >= startDate.Date && a.ScheduledDate <= endDate.Date)
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }
}
