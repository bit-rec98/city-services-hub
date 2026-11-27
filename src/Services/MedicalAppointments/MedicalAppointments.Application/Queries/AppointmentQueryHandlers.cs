using CityServicesHub.BuildingBlocks.Common.Application;
using CityServicesHub.BuildingBlocks.Common.Interfaces;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Application.Security;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Interfaces;

namespace MedicalAppointments.Application.Queries;

/// <summary>
/// Handler para obtener un turno por ID.
/// </summary>
public class GetAppointmentByIdQueryHandler : IQueryHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAppointmentByIdQueryHandler(
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUser)
    {
        _appointmentRepository = appointmentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<AppointmentDto>> Handle(
        GetAppointmentByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        
        if (appointment is null || !AppointmentAccess.CanAccess(appointment, _currentUser))
            return Result.Failure<AppointmentDto>(AppointmentAccess.AppointmentNotFound);

        return Result.Success(MapToDto(appointment));
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientDocumentNumber = appointment.PatientDocumentNumber,
            PatientPhoneNumber = appointment.PatientPhoneNumber,
            PatientEmail = appointment.PatientEmail,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.DoctorName,
            Specialty = appointment.Specialty.ToString(),
            HealthCenterId = appointment.HealthCenterId,
            HealthCenterName = appointment.HealthCenterName,
            ScheduledDate = appointment.ScheduledDate,
            StartTime = appointment.StartTime.ToString("HH:mm"),
            EndTime = appointment.EndTime.ToString("HH:mm"),
            DurationMinutes = appointment.DurationMinutes,
            Status = appointment.Status.ToString(),
            AttentionType = appointment.AttentionType.ToString(),
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            CancellationReason = appointment.CancellationReason,
            CreatedAt = appointment.CreatedAt,
            ConfirmedAt = appointment.ConfirmedAt,
            CancelledAt = appointment.CancelledAt,
            CompletedAt = appointment.CompletedAt
        };
    }
}

/// <summary>
/// Handler para obtener turnos de un paciente.
/// </summary>
public class GetPatientAppointmentsQueryHandler : IQueryHandler<GetPatientAppointmentsQuery, IEnumerable<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUser;

    public GetPatientAppointmentsQueryHandler(
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUser)
    {
        _appointmentRepository = appointmentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(
        GetPatientAppointmentsQuery request, 
        CancellationToken cancellationToken)
    {
        if (!AppointmentAccess.CanAccessPatient(request.PatientId, _currentUser))
            return Result.Failure<IEnumerable<AppointmentDto>>(AppointmentAccess.PatientForbidden);

        var appointments = request.UpcomingOnly
            ? await _appointmentRepository.GetUpcomingByPatientAsync(request.PatientId, cancellationToken)
            : await _appointmentRepository.GetByPatientAsync(request.PatientId, cancellationToken);

        var dtos = appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.PatientName,
            PatientDocumentNumber = a.PatientDocumentNumber,
            DoctorId = a.DoctorId,
            DoctorName = a.DoctorName,
            Specialty = a.Specialty.ToString(),
            HealthCenterId = a.HealthCenterId,
            HealthCenterName = a.HealthCenterName,
            ScheduledDate = a.ScheduledDate,
            StartTime = a.StartTime.ToString("HH:mm"),
            EndTime = a.EndTime.ToString("HH:mm"),
            DurationMinutes = a.DurationMinutes,
            Status = a.Status.ToString(),
            AttentionType = a.AttentionType.ToString(),
            Reason = a.Reason,
            CreatedAt = a.CreatedAt
        }).ToList();

        return Result.Success<IEnumerable<AppointmentDto>>(dtos);
    }
}

/// <summary>
/// Handler para obtener centros de salud.
/// </summary>
public class GetHealthCentersQueryHandler : IQueryHandler<GetHealthCentersQuery, IEnumerable<HealthCenterDto>>
{
    private readonly IHealthCenterRepository _healthCenterRepository;

    public GetHealthCentersQueryHandler(IHealthCenterRepository healthCenterRepository)
    {
        _healthCenterRepository = healthCenterRepository;
    }

    public async Task<Result<IEnumerable<HealthCenterDto>>> Handle(
        GetHealthCentersQuery request, 
        CancellationToken cancellationToken)
    {
        IEnumerable<HealthCenter> centers;

        if (!string.IsNullOrEmpty(request.City))
        {
            centers = await _healthCenterRepository.GetByCityAsync(request.City, cancellationToken);
        }
        else if (request.Specialty.HasValue)
        {
            centers = await _healthCenterRepository.GetBySpecialtyAsync(request.Specialty.Value, cancellationToken);
        }
        else if (request.ActiveOnly)
        {
            centers = await _healthCenterRepository.GetActiveAsync(cancellationToken);
        }
        else
        {
            centers = await _healthCenterRepository.GetAllAsync(cancellationToken);
        }

        var dtos = centers.Select(c => new HealthCenterDto
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Address = c.Address,
            City = c.City,
            Province = c.Province,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Latitude = c.Location?.Latitude,
            Longitude = c.Location?.Longitude,
            IsActive = c.IsActive,
            Specialties = c.Specialties.Select(s => s.ToString())
        }).ToList();

        return Result.Success<IEnumerable<HealthCenterDto>>(dtos);
    }
}
