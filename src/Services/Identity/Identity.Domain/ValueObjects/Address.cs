using CityServicesHub.BuildingBlocks.Common.Domain;

namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una dirección física.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string City { get; private set; }
    public string Province { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address(
        string street, 
        string number, 
        string? floor, 
        string? apartment, 
        string city, 
        string province, 
        string postalCode, 
        string country)
    {
        Street = street;
        Number = number;
        Floor = floor;
        Apartment = apartment;
        City = city;
        Province = province;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string street,
        string number,
        string? floor,
        string? apartment,
        string city,
        string province,
        string postalCode,
        string country = "Argentina")
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("La calle no puede estar vacía.", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("La ciudad no puede estar vacía.", nameof(city));
        
        if (string.IsNullOrWhiteSpace(province))
            throw new ArgumentException("La provincia no puede estar vacía.", nameof(province));

        return new Address(
            street.Trim(), 
            number?.Trim() ?? "S/N", 
            floor?.Trim(), 
            apartment?.Trim(), 
            city.Trim(), 
            province.Trim(), 
            postalCode?.Trim() ?? "", 
            country.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Floor;
        yield return Apartment;
        yield return City;
        yield return Province;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString()
    {
        var address = $"{Street} {Number}";
        if (!string.IsNullOrWhiteSpace(Floor))
            address += $", Piso {Floor}";
        if (!string.IsNullOrWhiteSpace(Apartment))
            address += $", Depto {Apartment}";
        address += $", {City}, {Province}";
        if (!string.IsNullOrWhiteSpace(PostalCode))
            address += $" ({PostalCode})";
        address += $", {Country}";
        return address;
    }
}
