using CityServices.Shared.Common;
using MediatR;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Application.Queries.GetAllAppointments;

public class GetAllAppointmentsQueryHandler : IRequestHandler<GetAllAppointmentsQuery, Result<IEnumerable<AppointmentDto>>>
{
    private readonly IRepository<Appointment> _appointmentRepository;

    public GetAllAppointmentsQueryHandler(IRepository<Appointment> appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);

        var dtos = appointments.Select(appointment => new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientEmail = appointment.PatientEmail,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.DoctorName,
            Specialty = appointment.Specialty,
            AppointmentDate = appointment.AppointmentDate,
            Duration = appointment.Duration,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        });

        return Result.Success(dtos);
    }
}
