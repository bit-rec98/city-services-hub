using CityServicesHub.BuildingBlocks.Common.Application;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Queries;

/// <summary>
/// Handler para obtener el conteo de notificaciones no leídas.
/// </summary>
public class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, UnreadCountDto>
{
    private readonly INotificationRepository _repository;

    public GetUnreadCountQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);
        return Result.Success(new UnreadCountDto { Count = count });
    }
}
