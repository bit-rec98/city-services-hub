using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;

namespace Notifications.Application.Queries;

/// <summary>
/// Query para obtener el conteo de notificaciones no leídas de un usuario.
/// </summary>
public record GetUnreadCountQuery(Guid UserId) : IQuery<UnreadCountDto>;
