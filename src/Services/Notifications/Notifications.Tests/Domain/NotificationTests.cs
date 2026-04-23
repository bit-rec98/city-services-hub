using Notifications.Domain.Entities;

namespace Notifications.Tests.Domain;

/// <summary>
/// Tests unitarios para la entidad Notification.
/// </summary>
public class NotificationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Test Notification";
        var content = "This is a test notification";

        // Act
        var notification = Notification.Create(
            userId,
            title,
            content,
            NotificationType.System,
            NotificationChannel.InApp);

        // Assert
        notification.Should().NotBeNull();
        notification.UserId.Should().Be(userId);
        notification.Title.Should().Be(title);
        notification.Content.Should().Be(content);
        notification.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public void Create_WithScheduledDate_ShouldSetScheduledStatus()
    {
        // Arrange
        var scheduledFor = DateTime.UtcNow.AddHours(1);

        // Act
        var notification = Notification.Create(
            Guid.NewGuid(),
            "Test",
            "Content",
            NotificationType.Reminder,
            NotificationChannel.Email,
            scheduledFor: scheduledFor);

        // Assert
        notification.Status.Should().Be(NotificationStatus.Scheduled);
        notification.ScheduledFor.Should().Be(scheduledFor);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        // Arrange
        var notification = CreateTestNotification();

        // Act
        notification.MarkAsSent();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsRead_ShouldUpdateStatusAndReadAt()
    {
        // Arrange
        var notification = CreateTestNotification();
        notification.MarkAsSent();

        // Act
        notification.MarkAsRead();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Read);
        notification.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldIncrementRetryCount()
    {
        // Arrange
        var notification = CreateTestNotification();

        // Act
        notification.MarkAsFailed("Test error");

        // Assert
        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.RetryCount.Should().Be(1);
        notification.LastError.Should().Be("Test error");
    }

    [Fact]
    public void Retry_WhenUnderRetryLimit_ShouldSetPendingStatus()
    {
        // Arrange
        var notification = CreateTestNotification();
        notification.MarkAsFailed("Error 1");

        // Act
        notification.Retry();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Pending);
    }

    [Fact]
    public void Retry_WhenOverRetryLimit_ShouldRemainFailed()
    {
        // Arrange
        var notification = CreateTestNotification();
        notification.MarkAsFailed("Error 1");
        notification.Retry();
        notification.MarkAsFailed("Error 2");
        notification.Retry();
        notification.MarkAsFailed("Error 3");

        // Act
        notification.Retry();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Failed);
    }

    private static Notification CreateTestNotification()
    {
        return Notification.Create(
            Guid.NewGuid(),
            "Test Notification",
            "Test Content",
            NotificationType.System,
            NotificationChannel.InApp);
    }
}
