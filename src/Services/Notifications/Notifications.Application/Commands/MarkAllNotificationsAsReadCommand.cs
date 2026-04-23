using CityServicesHub.BuildingBlocks.Common.Application;

namespace Notifications.Application.Commands;

/// <summary>
/// Comando para marcar todas las notificaciones de un usuario como leídas.
/// </summary>
public record MarkAllNotificationsAsReadCommand(Guid UserId) : ICommand;
