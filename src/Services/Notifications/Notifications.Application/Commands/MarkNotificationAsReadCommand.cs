using CityServicesHub.BuildingBlocks.Common.Application;

namespace Notifications.Application.Commands;

/// <summary>
/// Comando para marcar una notificación como leída.
/// </summary>
public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : ICommand;
