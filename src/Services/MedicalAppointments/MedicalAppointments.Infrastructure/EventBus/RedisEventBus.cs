using CityServices.Shared.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace MedicalAppointments.Infrastructure.EventBus;

public class RedisEventBus : IEventBus
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;

    public RedisEventBus(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        var channel = @event.EventType;
        var message = JsonSerializer.Serialize(@event);
        await _subscriber.PublishAsync(channel, message);
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        var eventType = typeof(T).Name;
        await _subscriber.SubscribeAsync(eventType, async (channel, message) =>
        {
            if (message.HasValue)
            {
                var @event = JsonSerializer.Deserialize<T>(message!);
                if (@event != null)
                {
                    await handler(@event);
                }
            }
        });
    }
}
