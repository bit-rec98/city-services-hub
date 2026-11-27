using Identity.Application.Commands;
using Identity.Application.Validators;
using Identity.Domain.Enums;

namespace Identity.Tests.Application;

/// <summary>
/// Tests de las reglas de validación de entrada de registro y login.
/// </summary>
public class AuthValidatorsTests
{
    private readonly RegisterUserCommandValidator _register = new();
    private readonly LoginCommandValidator _login = new();

    private static RegisterUserCommand ValidRegistration() => new(
        FirstName: "Ana",
        LastName: "López",
        Email: "ana@example.com",
        DocumentNumber: "12345678",
        DocumentType: (int)DocumentType.DNI,
        Password: "Str0ng!Pass",
        ConfirmPassword: "Str0ng!Pass");

    [Fact]
    public void Register_WithValidData_IsValid() =>
        _register.Validate(ValidRegistration()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData((int)DocumentType.DNI)]
    [InlineData((int)DocumentType.CUIT)]
    [InlineData((int)DocumentType.CUIL)]
    [InlineData((int)DocumentType.Passport)]
    [InlineData((int)DocumentType.ForeignId)]
    public void Register_WithEveryDefinedDocumentType_IsValid(int documentType) =>
        _register.Validate(ValidRegistration() with { DocumentType = documentType }).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void Register_WithUndefinedDocumentType_IsInvalid(int documentType) =>
        _register.Validate(ValidRegistration() with { DocumentType = documentType }).IsValid.Should().BeFalse();

    [Fact]
    public void Register_WithMismatchedPasswords_IsInvalid() =>
        _register.Validate(ValidRegistration() with { ConfirmPassword = "Different1!" }).IsValid.Should().BeFalse();

    [Fact]
    public void Register_WithWeakPassword_IsInvalid() =>
        _register.Validate(ValidRegistration() with { Password = "weak", ConfirmPassword = "weak" }).IsValid.Should().BeFalse();

    [Fact]
    public void Login_WithEmailAndPassword_IsValid() =>
        _login.Validate(new LoginCommand("ana@example.com", "x")).IsValid.Should().BeTrue();

    [Fact]
    public void Login_WithoutPassword_IsInvalid() =>
        _login.Validate(new LoginCommand("ana@example.com", "")).IsValid.Should().BeFalse();
}
