using CityServicesHub.BuildingBlocks.Common.Domain;

namespace Notifications.Domain.Entities;

/// <summary>
/// Entidad que representa una notificación enviada al usuario.
/// </summary>
public class Notification : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? Recipient { get; private set; }
    public string? TemplateId { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public Dictionary<string, string>? Metadata { get; private set; }

    protected Notification() { }

    public static Notification Create(
        Guid userId,
        string title,
        string content,
        NotificationType type,
        NotificationChannel channel,
        NotificationPriority priority = NotificationPriority.Normal,
        string? recipient = null,
        string? templateId = null,
        string? referenceType = null,
        Guid? referenceId = null,
        DateTime? scheduledFor = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            Channel = channel,
            Priority = priority,
            Status = scheduledFor.HasValue ? NotificationStatus.Scheduled : NotificationStatus.Pending,
            Recipient = recipient,
            TemplateId = templateId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ScheduledFor = scheduledFor,
            RetryCount = 0
        };
        notification.SetCreatedInfo(null);
        return notification;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        SetUpdatedInfo(null);
    }

    public void MarkAsRead()
    {
        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
        SetUpdatedInfo(null);
    }

    public void MarkAsFailed(string error)
    {
        Status = NotificationStatus.Failed;
        LastError = error;
        RetryCount++;
        SetUpdatedInfo(null);
    }

    public void Retry()
    {
        if (RetryCount >= 3)
        {
            Status = NotificationStatus.Failed;
            return;
        }

        Status = NotificationStatus.Pending;
        SetUpdatedInfo(null);
    }
}

public enum NotificationType
{
    System = 0,
    Appointment = 1,
    Payment = 2,
    Document = 3,
    Alert = 4,
    Reminder = 5,
    Marketing = 6
}

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    SMS = 2,
    Push = 3,
    WhatsApp = 4
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Scheduled = 1,
    Sent = 2,
    Read = 3,
    Failed = 4,
    Cancelled = 5
}
