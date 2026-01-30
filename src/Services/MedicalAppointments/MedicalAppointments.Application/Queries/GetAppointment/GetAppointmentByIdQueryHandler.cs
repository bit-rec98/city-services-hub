using CityServices.Shared.Common;
using MediatR;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Domain.Entities;

namespace MedicalAppointments.Application.Queries.GetAppointment;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly IRepository<Appointment> _appointmentRepository;

    public GetAppointmentByIdQueryHandler(IRepository<Appointment> appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (appointment == null)
        {
            return Result.Failure<AppointmentDto>("Appointment not found");
        }

        var dto = new AppointmentDto
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
        };

        return Result.Success(dto);
    }
}
