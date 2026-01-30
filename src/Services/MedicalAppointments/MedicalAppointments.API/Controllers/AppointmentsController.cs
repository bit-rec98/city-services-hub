using MediatR;
using MedicalAppointments.Application.Commands.CreateAppointment;
using MedicalAppointments.Application.Queries.GetAppointment;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetAppointment), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var result = await _mediator.Send(new GetAppointmentByIdQuery(id));
        
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
