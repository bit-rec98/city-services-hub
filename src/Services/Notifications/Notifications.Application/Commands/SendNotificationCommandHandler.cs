using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Application.Services;
using Notifications.Domain.Entities;

namespace Notifications.Application.Commands;

/// <summary>
/// Handler para envío de notificaciones.
/// </summary>
public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationDispatcher _dispatcher;

    public SendNotificationCommandHandler(
        INotificationRepository repository,
        INotificationDispatcher dispatcher)
    {
        _repository = repository;
        _dispatcher = dispatcher;
    }

    public async Task<Result<NotificationDto>> Handle(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = Notification.Create(
            request.UserId,
            request.Title,
            request.Content,
            request.Type,
            request.Channel,
            request.Priority,
            request.Recipient,
            request.TemplateId,
            request.ReferenceType,
            request.ReferenceId,
            request.ScheduledFor);

        await _repository.AddAsync(notification, cancellationToken);

        // Only dispatch immediately if not scheduled for later
        if (!request.ScheduledFor.HasValue)
        {
            await _dispatcher.DispatchAsync(notification, cancellationToken);
            await _repository.UpdateAsync(notification, cancellationToken);
        }

        return Result.Success(MapToDto(notification));
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Title = n.Title,
        Content = n.Content,
        Type = n.Type,
        Channel = n.Channel,
        Priority = n.Priority,
        Status = n.Status,
        ReferenceType = n.ReferenceType,
        ReferenceId = n.ReferenceId,
        SentAt = n.SentAt,
        ReadAt = n.ReadAt,
        ScheduledFor = n.ScheduledFor,
        CreatedAt = n.CreatedAt
    };
}
