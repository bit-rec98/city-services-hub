using CityServicesHub.BuildingBlocks.Common.Domain;

namespace MedicalAppointments.Domain.ValueObjects;

/// <summary>
/// Value Object para coordenadas geográficas.
/// </summary>
public class GeoLocation : ValueObject
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("La latitud debe estar entre -90 y 90.", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("La longitud debe estar entre -180 y 180.", nameof(longitude));

        return new GeoLocation(latitude, longitude);
    }

    /// <summary>
    /// Calcula la distancia aproximada en kilómetros usando la fórmula de Haversine.
    /// </summary>
    public double DistanceTo(GeoLocation other)
    {
        const double R = 6371; // Radio de la Tierra en km

        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"({Latitude}, {Longitude})";
}
