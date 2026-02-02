using CityServicesHub.BuildingBlocks.Common.Domain;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Domain.Events;
using MedicalAppointments.Domain.ValueObjects;

namespace MedicalAppointments.Domain.Entities;

/// <summary>
/// Entidad que representa un centro de salud.
/// </summary>
public class HealthCenter : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Province { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public GeoLocation? Location { get; private set; }
    public bool IsActive { get; private set; }
    
    private readonly List<MedicalSpecialty> _specialties = new();
    public IReadOnlyCollection<MedicalSpecialty> Specialties => _specialties.AsReadOnly();

    private readonly List<Doctor> _doctors = new();
    public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();

    private HealthCenter() { }

    private HealthCenter(
        Guid id,
        string name,
        string code,
        string address,
        string city,
        string province) : base(id)
    {
        Name = name;
        Code = code;
        Address = address;
        City = city;
        Province = province;
        IsActive = true;
    }

    public static HealthCenter Create(
        string name,
        string code,
        string address,
        string city,
        string province)
    {
        var center = new HealthCenter(Guid.NewGuid(), name, code, address, city, province);
        center.AddDomainEvent(new HealthCenterCreatedEvent(center.Id, name, code));
        return center;
    }

    public void UpdateInfo(
        string name,
        string address,
        string city,
        string? phoneNumber,
        string? email)
    {
        Name = name;
        Address = address;
        City = city;
        PhoneNumber = phoneNumber;
        Email = email;
    }

    public void SetLocation(double latitude, double longitude)
    {
        Location = GeoLocation.Create(latitude, longitude);
    }

    public void AddSpecialty(MedicalSpecialty specialty)
    {
        if (!_specialties.Contains(specialty))
            _specialties.Add(specialty);
    }

    public void RemoveSpecialty(MedicalSpecialty specialty)
    {
        _specialties.Remove(specialty);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
