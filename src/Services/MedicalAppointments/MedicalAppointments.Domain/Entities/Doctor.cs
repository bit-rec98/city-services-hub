using CityServicesHub.BuildingBlocks.Common.Domain;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.ValueObjects;

namespace MedicalAppointments.Domain.Entities;

/// <summary>
/// Entidad que representa un profesional médico.
/// </summary>
public class Doctor : AggregateRoot
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = null!;
    public MedicalSpecialty PrimarySpecialty { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }
    
    // Duración estándar de turno en minutos
    public int DefaultAppointmentDuration { get; private set; }
    
    private readonly List<MedicalSpecialty> _secondarySpecialties = new();
    public IReadOnlyCollection<MedicalSpecialty> SecondarySpecialties => _secondarySpecialties.AsReadOnly();
    
    private readonly List<DoctorSchedule> _schedules = new();
    public IReadOnlyCollection<DoctorSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<Guid> _healthCenterIds = new();
    public IReadOnlyCollection<Guid> HealthCenterIds => _healthCenterIds.AsReadOnly();

    private Doctor() { }

    private Doctor(
        Guid id,
        string firstName,
        string lastName,
        string licenseNumber,
        MedicalSpecialty primarySpecialty) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        LicenseNumber = licenseNumber;
        PrimarySpecialty = primarySpecialty;
        IsActive = true;
        DefaultAppointmentDuration = 20; // 20 minutos por defecto
    }

    public static Doctor Create(
        string firstName,
        string lastName,
        string licenseNumber,
        MedicalSpecialty primarySpecialty)
    {
        return new Doctor(Guid.NewGuid(), firstName, lastName, licenseNumber, primarySpecialty);
    }

    public string FullName => $"Dr./Dra. {FirstName} {LastName}";

    public void UpdateContactInfo(string? email, string? phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void AddSecondarySpecialty(MedicalSpecialty specialty)
    {
        if (specialty != PrimarySpecialty && !_secondarySpecialties.Contains(specialty))
            _secondarySpecialties.Add(specialty);
    }

    public void RemoveSecondarySpecialty(MedicalSpecialty specialty)
    {
        _secondarySpecialties.Remove(specialty);
    }

    public void AssignToHealthCenter(Guid healthCenterId)
    {
        if (!_healthCenterIds.Contains(healthCenterId))
            _healthCenterIds.Add(healthCenterId);
    }

    public void RemoveFromHealthCenter(Guid healthCenterId)
    {
        _healthCenterIds.Remove(healthCenterId);
    }

    public void SetDefaultAppointmentDuration(int minutes)
    {
        if (minutes < 5 || minutes > 120)
            throw new ArgumentException("La duración del turno debe estar entre 5 y 120 minutos.");
        
        DefaultAppointmentDuration = minutes;
    }

    public void AddSchedule(DoctorSchedule schedule)
    {
        // Verificar que no haya superposición
        foreach (var existing in _schedules)
        {
            if (existing.HealthCenterId == schedule.HealthCenterId &&
                existing.DayOfWeek == schedule.DayOfWeek &&
                existing.TimeSlot.Overlaps(schedule.TimeSlot))
            {
                throw new InvalidOperationException("El horario se superpone con otro existente.");
            }
        }
        
        _schedules.Add(schedule);
    }

    public void RemoveSchedule(Guid scheduleId)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule != null)
            _schedules.Remove(schedule);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

/// <summary>
/// Entidad que representa el horario de atención de un médico.
/// </summary>
public class DoctorSchedule : Entity
{
    public Guid DoctorId { get; private set; }
    public Guid HealthCenterId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = null!;
    public int MaxAppointmentsPerSlot { get; private set; }
    public bool IsActive { get; private set; }

    private DoctorSchedule() { }

    public DoctorSchedule(
        Guid doctorId,
        Guid healthCenterId,
        DayOfWeek dayOfWeek,
        TimeSlot timeSlot,
        int maxAppointmentsPerSlot = 1) : base(Guid.NewGuid())
    {
        DoctorId = doctorId;
        HealthCenterId = healthCenterId;
        DayOfWeek = dayOfWeek;
        TimeSlot = timeSlot;
        MaxAppointmentsPerSlot = maxAppointmentsPerSlot;
        IsActive = true;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
