using CityServicesHub.BuildingBlocks.Common.Interfaces;
using Notifications.Domain.Entities;

namespace Notifications.Application.Interfaces;

/// <summary>
/// Repositorio para gestión de notificaciones.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>Obtiene notificaciones de un usuario con paginación.</summary>
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId,
        bool unreadOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Cuenta notificaciones no leídas de un usuario.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Obtiene todas las notificaciones pendientes de envío.</summary>
    Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene notificaciones programadas cuyo tiempo de envío llegó.</summary>
    Task<IReadOnlyList<Notification>> GetDueScheduledNotificationsAsync(
        CancellationToken cancellationToken = default);
}
