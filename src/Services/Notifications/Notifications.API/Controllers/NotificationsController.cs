using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace Notifications.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(ILogger<NotificationsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las notificaciones del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Implementar lógica de obtención de notificaciones
        _logger.LogInformation(
            "Obteniendo notificaciones: unreadOnly={UnreadOnly}, page={Page}", 
            unreadOnly, page);

        return Ok(new
        {
            Items = Array.Empty<object>(),
            TotalCount = 0,
            PageNumber = page,
            PageSize = pageSize,
            UnreadCount = 0
        });
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        _logger.LogInformation("Marcando notificación {Id} como leída", id);
        return Ok();
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas.
    /// </summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        _logger.LogInformation("Marcando todas las notificaciones como leídas");
        return Ok();
    }

    /// <summary>
    /// Obtiene el contador de notificaciones no leídas.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        return Ok(new { Count = 0 });
    }
}
