using Identity.Domain.ValueObjects;

namespace Identity.Tests.Domain;

/// <summary>
/// Tests unitarios para los Value Objects.
/// </summary>
public class ValueObjectsTests
{
    #region Email Tests

    [Theory]
    [InlineData("test@email.com")]
    [InlineData("user.name@domain.co.ar")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Email_Create_WithValidEmail_ShouldCreate(string emailValue)
    {
        // Act
        var email = Email.Create(emailValue);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(emailValue.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Email_Create_WithEmptyValue_ShouldThrowException(string? emailValue)
    {
        // Act
        var act = () => Email.Create(emailValue!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("@nodomain.com")]
    public void Email_Create_WithInvalidFormat_ShouldThrowException(string emailValue)
    {
        // Act
        var act = () => Email.Create(emailValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Email_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var email1 = Email.Create("test@email.com");
        var email2 = Email.Create("TEST@EMAIL.COM");
        var email3 = Email.Create("other@email.com");

        // Assert
        email1.Should().Be(email2);
        email1.Should().NotBe(email3);
    }

    #endregion

    #region DocumentNumber Tests

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567")]
    [InlineData("12.345.678")]
    [InlineData("12 345 678")]
    public void DocumentNumber_Create_WithValidNumber_ShouldCreate(string docValue)
    {
        // Act
        var doc = DocumentNumber.Create(docValue);

        // Assert
        doc.Should().NotBeNull();
        doc.Value.Should().MatchRegex(@"^\d{7,8}$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void DocumentNumber_Create_WithEmptyValue_ShouldThrowException(string? docValue)
    {
        // Act
        var act = () => DocumentNumber.Create(docValue!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("123456")]   // muy corto
    [InlineData("123456789")] // muy largo
    [InlineData("1234567A")] // contiene letra
    public void DocumentNumber_Create_WithInvalidFormat_ShouldThrowException(string docValue)
    {
        // Act
        var act = () => DocumentNumber.Create(docValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DocumentNumber_ToFormattedString_ShouldFormatCorrectly()
    {
        // Arrange
        var doc8 = DocumentNumber.Create("12345678");
        var doc7 = DocumentNumber.Create("1234567");

        // Act & Assert
        doc8.ToFormattedString().Should().Be("12.345.678");
        doc7.ToFormattedString().Should().Be("1.234.567");
    }

    #endregion

    #region PhoneNumber Tests

    [Theory]
    [InlineData("+54 351 1234567")]
    [InlineData("543511234567")]
    [InlineData("3511234567")]
    public void PhoneNumber_Create_WithValidNumber_ShouldCreate(string phoneValue)
    {
        // Act
        var phone = PhoneNumber.Create(phoneValue);

        // Assert
        phone.Should().NotBeNull();
        phone.CountryCode.Should().Be("+54");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void PhoneNumber_Create_WithEmptyValue_ShouldThrowException(string? phoneValue)
    {
        // Act
        var act = () => PhoneNumber.Create(phoneValue!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PhoneNumber_Create_WithShortNumber_ShouldThrowException()
    {
        // Act
        var act = () => PhoneNumber.Create("123456");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Address Tests

    [Fact]
    public void Address_Create_WithValidData_ShouldCreate()
    {
        // Act
        var address = Address.Create(
            street: "Av. Colón",
            number: "1234",
            floor: "3",
            apartment: "A",
            city: "Córdoba",
            province: "Córdoba",
            postalCode: "5000");

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be("Av. Colón");
        address.City.Should().Be("Córdoba");
        address.Country.Should().Be("Argentina");
    }

    [Fact]
    public void Address_Create_WithEmptyStreet_ShouldThrowException()
    {
        // Act
        var act = () => Address.Create(
            street: "",
            number: "1234",
            floor: null,
            apartment: null,
            city: "Córdoba",
            province: "Córdoba",
            postalCode: "5000");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Address_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var address = Address.Create(
            street: "Av. Colón",
            number: "1234",
            floor: "3",
            apartment: "A",
            city: "Córdoba",
            province: "Córdoba",
            postalCode: "5000");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Contain("Av. Colón 1234");
        result.Should().Contain("Piso 3");
        result.Should().Contain("Depto A");
        result.Should().Contain("Córdoba");
    }

    #endregion
}
