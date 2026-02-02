namespace CityServicesHub.BuildingBlocks.Common.Interfaces;

/// <summary>
/// Interface para servicios de fecha y hora.
/// Facilita el testing al permitir mockear la hora actual.
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTimeOffset NowOffset { get; }
    DateTimeOffset UtcNowOffset { get; }
}

/// <summary>
/// Implementación por defecto del proveedor de fecha/hora.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset NowOffset => DateTimeOffset.Now;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
