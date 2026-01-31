using CityServices.Shared.Common;
using MediatR;
using MedicalAppointments.Application.DTOs;

namespace MedicalAppointments.Application.Queries.GetAppointment;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDto>>;
