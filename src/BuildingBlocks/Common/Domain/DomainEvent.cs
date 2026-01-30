using MediatR;

namespace CityServicesHub.BuildingBlocks.Common.Domain;

/// <summary>
/// Interface marcador para eventos de dominio.
/// Los eventos de dominio representan algo que sucedió en el dominio.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

/// <summary>
/// Implementación base de evento de dominio.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
