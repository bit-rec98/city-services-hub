using CityServicesHub.BuildingBlocks.Common.Domain;

namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un número de teléfono.
/// </summary>
public class PhoneNumber : ValueObject
{
    public string CountryCode { get; private set; }
    public string AreaCode { get; private set; }
    public string Number { get; private set; }

    private PhoneNumber(string countryCode, string areaCode, string number)
    {
        CountryCode = countryCode;
        AreaCode = areaCode;
        Number = number;
    }

    public static PhoneNumber Create(string fullNumber)
    {
        if (string.IsNullOrWhiteSpace(fullNumber))
            throw new ArgumentException("El número de teléfono no puede estar vacío.", nameof(fullNumber));

        // Remover caracteres no numéricos excepto el signo +
        var cleaned = new string(fullNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Argentina: +54 (código país) + código de área + número
        // Ejemplo: +54 351 1234567 (Córdoba)
        if (cleaned.StartsWith("+54"))
        {
            cleaned = cleaned[3..];
        }
        else if (cleaned.StartsWith("54"))
        {
            cleaned = cleaned[2..];
        }

        if (cleaned.Length < 10)
            throw new ArgumentException("El número de teléfono debe tener al menos 10 dígitos.", nameof(fullNumber));

        return new PhoneNumber("+54", cleaned[..3], cleaned[3..]);
    }

    public static PhoneNumber Create(string countryCode, string areaCode, string number)
    {
        return new PhoneNumber(countryCode, areaCode, number);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return AreaCode;
        yield return Number;
    }

    public override string ToString() => $"{CountryCode} {AreaCode} {Number}";

    public string ToE164Format() => $"{CountryCode}{AreaCode}{Number}".Replace("+", "");
}
