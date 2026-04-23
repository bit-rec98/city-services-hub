using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;
using Notifications.Domain.Entities;

namespace Notifications.Application.Commands;

/// <summary>
/// Comando para enviar una nueva notificación.
/// </summary>
public record SendNotificationCommand(
    Guid UserId,
    string Title,
    string Content,
    NotificationType Type,
    NotificationChannel Channel,
    NotificationPriority Priority = NotificationPriority.Normal,
    string? Recipient = null,
    string? TemplateId = null,
    string? ReferenceType = null,
    Guid? ReferenceId = null,
    DateTime? ScheduledFor = null
) : ICommand<NotificationDto>;
