using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;

namespace Notifications.Application.Queries;

/// <summary>
/// Query para obtener las notificaciones de un usuario con paginación.
/// </summary>
public record GetUserNotificationsQuery(
    Guid UserId,
    bool UnreadOnly = false,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<PagedResult<NotificationDto>>;
