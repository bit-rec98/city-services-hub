using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;

namespace Identity.Tests.Domain;

/// <summary>
/// Tests unitarios para la entidad User.
/// </summary>
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var firstName = "Juan";
        var lastName = "Pérez";
        var email = "juan.perez@email.com";
        var documentNumber = "12345678";
        var documentType = DocumentType.DNI;
        var passwordHash = "hashedPassword123";

        // Act
        var user = User.Create(
            firstName, 
            lastName, 
            email, 
            documentNumber, 
            documentType, 
            passwordHash);

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Value.Should().Be(email);
        user.DocumentNumber.Value.Should().Be(documentNumber);
        user.DocumentType.Should().Be(documentType);
        user.Status.Should().Be(UserStatus.Pending);
        user.EmailVerified.Should().BeFalse();
        user.Roles.Should().Contain(UserRole.Citizen);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Act
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().EventType.Should().Be("UserCreated");
    }

    [Fact]
    public void FullName_ShouldReturnCombinedName()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("Juan Pérez");
    }

    [Fact]
    public void VerifyEmail_ShouldSetEmailVerifiedAndActivateUser()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        user.VerifyEmail();

        // Assert
        user.EmailVerified.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void AddRole_ShouldAddNewRole()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        user.AddRole(UserRole.Administrator);

        // Assert
        user.Roles.Should().Contain(UserRole.Administrator);
        user.Roles.Should().Contain(UserRole.Citizen);
    }

    [Fact]
    public void AddRole_WhenRoleAlreadyExists_ShouldNotDuplicate()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        user.AddRole(UserRole.Citizen);
        user.AddRole(UserRole.Citizen);

        // Assert
        user.Roles.Count(r => r == UserRole.Citizen).Should().Be(1);
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementCounter()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(2);
        user.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_After5Attempts_ShouldLockAccount()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Assert
        user.IsLockedOut.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public void RecordLogin_ShouldResetFailedAttemptsAndUpdateLastLogin()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.RecordLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAt.Should().NotBeNull();
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void AddRefreshToken_ShouldAddToken()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");

        // Act
        var token = user.AddRefreshToken("test_token_123", DateTime.UtcNow.AddDays(7));

        // Assert
        user.RefreshTokens.Should().HaveCount(1);
        token.Token.Should().Be("test_token_123");
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeToken()
    {
        // Arrange
        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hash");
        user.AddRefreshToken("test_token_123", DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeRefreshToken("test_token_123");

        // Assert
        var token = user.RefreshTokens.First();
        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }
}
