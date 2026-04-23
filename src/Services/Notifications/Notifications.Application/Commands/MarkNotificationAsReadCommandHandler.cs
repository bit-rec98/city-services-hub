using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Commands;

/// <summary>
/// Handler para marcar una notificación como leída.
/// </summary>
public class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand>
{
    private readonly INotificationRepository _repository;

    public MarkNotificationAsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(Error.NotFound);

        if (notification.UserId != request.UserId)
            return Result.Failure(Error.Forbidden);

        notification.MarkAsRead();
        await _repository.UpdateAsync(notification, cancellationToken);

        return Result.Success();
    }
}
