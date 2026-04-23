namespace CityServicesHub.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Evento de integración para comunicación entre microservicios.
/// </summary>
public record IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreationDate { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
