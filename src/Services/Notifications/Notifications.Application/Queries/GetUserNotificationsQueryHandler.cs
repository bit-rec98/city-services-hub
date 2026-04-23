using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;

namespace Notifications.Application.Queries;

/// <summary>
/// Handler para obtener las notificaciones de un usuario.
/// </summary>
public class GetUserNotificationsQueryHandler
    : IQueryHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetUserNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetByUserIdAsync(
            request.UserId,
            request.UnreadOnly,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return Result.Success(new PagedResult<NotificationDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize));
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
