using CityServicesHub.BuildingBlocks.Common.Domain;

namespace Identity.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un número de documento argentino (DNI).
/// </summary>
public class DocumentNumber : ValueObject
{
    public string Value { get; private set; }

    private DocumentNumber(string value)
    {
        Value = value;
    }

    public static DocumentNumber Create(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("El número de documento no puede estar vacío.", nameof(documentNumber));

        // Remover puntos y espacios
        documentNumber = documentNumber.Replace(".", "").Replace(" ", "").Trim();

        if (!IsValidDni(documentNumber))
            throw new ArgumentException("El número de documento no es válido.", nameof(documentNumber));

        return new DocumentNumber(documentNumber);
    }

    private static bool IsValidDni(string dni)
    {
        // DNI argentino: entre 7 y 8 dígitos numéricos
        return dni.Length >= 7 && dni.Length <= 8 && dni.All(char.IsDigit);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>
    /// Formatea el DNI con puntos (ej: 12.345.678)
    /// </summary>
    public string ToFormattedString()
    {
        if (Value.Length == 8)
            return $"{Value[..2]}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}";
        if (Value.Length == 7)
            return $"{Value[0]}.{Value.Substring(1, 3)}.{Value.Substring(4, 3)}";
        return Value;
    }

    public static implicit operator string(DocumentNumber doc) => doc.Value;
}
