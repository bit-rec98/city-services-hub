using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalAppointments.Application.Commands;
using MedicalAppointments.Application.DTOs;
using MedicalAppointments.Application.Queries;

namespace MedicalAppointments.API.Controllers;

/// <summary>
/// Controller para gestión de turnos médicos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IMediator mediator, ILogger<AppointmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo turno médico.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto request)
    {
        var command = new CreateAppointmentCommand(
            request.PatientId,
            request.PatientName,
            request.PatientDocumentNumber,
            request.PatientPhoneNumber,
            request.PatientEmail,
            request.DoctorId,
            request.HealthCenterId,
            request.Specialty,
            request.ScheduledDate,
            request.StartTime,
            request.DurationMinutes,
            request.AttentionType,
            request.Reason);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Appointment.Conflict" => Conflict(new { message = result.Error.Message }),
                _ => BadRequest(new { message = result.Error.Message })
            };
        }

        _logger.LogInformation("Turno creado: {AppointmentId}", result.Value.Id);
        return CreatedAtAction(nameof(GetAppointment), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Obtiene un turno por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var query = new GetAppointmentByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene los turnos de un paciente.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientAppointments(
        Guid patientId, 
        [FromQuery] bool upcomingOnly = false)
    {
        var query = new GetPatientAppointmentsQuery(patientId, upcomingOnly);
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }

    /// <summary>
    /// Confirma un turno.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmAppointment(Guid id)
    {
        var command = new ConfirmAppointmentCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Appointment.NotFound" => NotFound(new { message = result.Error.Message }),
                _ => BadRequest(new { message = result.Error.Message })
            };
        }

        return Ok(new { message = "Turno confirmado exitosamente." });
    }

    /// <summary>
    /// Cancela un turno.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        var command = new CancelAppointmentCommand(id, request.Reason);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Appointment.NotFound" => NotFound(new { message = result.Error.Message }),
                "Appointment.CannotCancel" => BadRequest(new { message = result.Error.Message }),
                _ => BadRequest(new { message = result.Error.Message })
            };
        }

        _logger.LogInformation("Turno cancelado: {AppointmentId}", id);
        return Ok(new { message = "Turno cancelado exitosamente." });
    }
}

/// <summary>
/// Controller para centros de salud.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HealthCentersController : ControllerBase
{
    private readonly IMediator _mediator;

    public HealthCentersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene los centros de salud.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HealthCenterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthCenters(
        [FromQuery] string? city = null,
        [FromQuery] int? specialty = null)
    {
        var query = new GetHealthCentersQuery(
            city, 
            specialty.HasValue ? (MedicalAppointments.Domain.Enums.MedicalSpecialty)specialty : null);
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }
}

public record CancelAppointmentRequest(string Reason);
