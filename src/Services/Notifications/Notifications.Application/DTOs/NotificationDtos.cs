using Notifications.Domain.Entities;

namespace Notifications.Application.DTOs;

/// <summary>
/// DTO para representar una notificación.
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = null!;
    public string Content { get; init; } = null!;
    public NotificationType Type { get; init; }
    public NotificationChannel Channel { get; init; }
    public NotificationPriority Priority { get; init; }
    public NotificationStatus Status { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime? ScheduledFor { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsRead => Status == NotificationStatus.Read;
}

/// <summary>
/// DTO para enviar una nueva notificación.
/// </summary>
public record SendNotificationDto
{
    public Guid UserId { get; init; }
    public string Title { get; init; } = null!;
    public string Content { get; init; } = null!;
    public NotificationType Type { get; init; }
    public NotificationChannel Channel { get; init; }
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public string? Recipient { get; init; }
    public string? TemplateId { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? ScheduledFor { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// DTO para respuesta de conteo de no leídas.
/// </summary>
public record UnreadCountDto
{
    public int Count { get; init; }
}
