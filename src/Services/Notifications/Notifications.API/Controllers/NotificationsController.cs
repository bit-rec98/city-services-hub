using CityServicesHub.BuildingBlocks.Common.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Notifications.Application.Commands;
using Notifications.Application.Queries;
using System.Security.Claims;

namespace Notifications.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not present."));

    /// <summary>
    /// Obtiene las notificaciones del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserNotificationsQuery(CurrentUserId, unreadOnly, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { result.Error.Code, result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene el contador de notificaciones no leídas.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var query = new GetUnreadCountQuery(CurrentUserId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { result.Error.Code, result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand(id, CurrentUserId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == Error.NotFound) return NotFound();
            if (result.Error == Error.Forbidden) return Forbid();
            return BadRequest(new { result.Error.Code, result.Error.Message });
        }

        return NoContent();
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var command = new MarkAllNotificationsAsReadCommand(CurrentUserId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { result.Error.Code, result.Error.Message });

        return NoContent();
    }
}
