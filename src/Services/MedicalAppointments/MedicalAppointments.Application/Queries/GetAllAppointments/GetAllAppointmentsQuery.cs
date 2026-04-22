using CityServices.Shared.Common;
using MediatR;
using MedicalAppointments.Application.DTOs;

namespace MedicalAppointments.Application.Queries.GetAllAppointments;

public record GetAllAppointmentsQuery : IRequest<Result<IEnumerable<AppointmentDto>>>;
