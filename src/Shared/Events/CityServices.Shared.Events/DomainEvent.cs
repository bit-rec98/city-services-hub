namespace CityServices.Shared.Events;

public abstract class DomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public string EventType { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }
}
