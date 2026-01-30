using CityServicesHub.BuildingBlocks.Common.Application;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Enums;

namespace MedicalAppointments.Application.Queries;

/// <summary>
/// Query para obtener un turno por ID.
/// </summary>
public record GetAppointmentByIdQuery(Guid AppointmentId) : IQuery<AppointmentDto>;

/// <summary>
/// Query para obtener turnos de un paciente.
/// </summary>
public record GetPatientAppointmentsQuery(
    Guid PatientId,
    bool UpcomingOnly = false
) : IQuery<IEnumerable<AppointmentDto>>;

/// <summary>
/// Query para obtener turnos de un médico en una fecha.
/// </summary>
public record GetDoctorAppointmentsQuery(
    Guid DoctorId,
    DateTime Date
) : IQuery<IEnumerable<AppointmentDto>>;

/// <summary>
/// Query para obtener centros de salud.
/// </summary>
public record GetHealthCentersQuery(
    string? City = null,
    MedicalSpecialty? Specialty = null,
    bool ActiveOnly = true
) : IQuery<IEnumerable<HealthCenterDto>>;

/// <summary>
/// Query para obtener médicos.
/// </summary>
public record GetDoctorsQuery(
    MedicalSpecialty? Specialty = null,
    Guid? HealthCenterId = null,
    bool ActiveOnly = true
) : IQuery<IEnumerable<DoctorDto>>;

/// <summary>
/// Query para obtener slots disponibles.
/// </summary>
public record GetAvailableSlotsQuery(
    Guid DoctorId,
    Guid HealthCenterId,
    DateTime Date,
    MedicalSpecialty? Specialty = null
) : IQuery<IEnumerable<AvailableSlotDto>>;
