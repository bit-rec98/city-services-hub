using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Commands;

/// <summary>
/// Handler para marcar todas las notificaciones como leídas.
/// </summary>
public class MarkAllNotificationsAsReadCommandHandler : ICommandHandler<MarkAllNotificationsAsReadCommand>
{
    private readonly INotificationRepository _repository;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var (notifications, _) = await _repository.GetByUserIdAsync(
            request.UserId,
            unreadOnly: true,
            pageNumber: 1,
            pageSize: 500,
            cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
            await _repository.UpdateAsync(notification, cancellationToken);
        }

        return Result.Success();
    }
}
