using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces;
using CityServicesHub.BuildingBlocks.Common.Application;

namespace Identity.Tests.Application;

/// <summary>
/// Tests unitarios para los Command Handlers de autenticación.
/// </summary>
public class AuthCommandHandlersTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock;

    public AuthCommandHandlersTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _verificationCodeServiceMock = new Mock<IVerificationCodeService>();
    }

    #region RegisterUserCommandHandler Tests

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new RegisterUserCommand(
            FirstName: "Juan",
            LastName: "Pérez",
            Email: "juan@email.com",
            DocumentNumber: "12345678",
            DocumentType: (int)DocumentType.DNI,
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _userRepositoryMock
            .Setup(x => x.DocumentNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        _verificationCodeServiceMock
            .Setup(x => x.GenerateToken())
            .Returns("verification_token");

        _verificationCodeServiceMock
            .Setup(x => x.StoreCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        var handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _verificationCodeServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("juan@email.com");
        result.Value.Status.Should().Be(UserStatus.Pending);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WithExistingEmail_ShouldFail()
    {
        // Arrange
        var command = new RegisterUserCommand(
            FirstName: "Juan",
            LastName: "Pérez",
            Email: "existing@email.com",
            DocumentNumber: "12345678",
            DocumentType: (int)DocumentType.DNI,
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync("existing@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _verificationCodeServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailExists");
    }

    [Fact]
    public async Task RegisterUser_WithExistingDocument_ShouldFail()
    {
        // Arrange
        var command = new RegisterUserCommand(
            FirstName: "Juan",
            LastName: "Pérez",
            Email: "juan@email.com",
            DocumentNumber: "12345678",
            DocumentType: (int)DocumentType.DNI,
            Password: "Password123!",
            ConfirmPassword: "Password123!");

        _userRepositoryMock
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _userRepositoryMock
            .Setup(x => x.DocumentNumberExistsAsync("12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _verificationCodeServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.DocumentExists");
    }

    #endregion

    #region LoginCommandHandler Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "juan@email.com",
            Password: "Password123!");

        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hashed_password");
        user.VerifyEmail(); // Activar usuario

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("juan@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("Password123!", "hashed_password"))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access_token");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.User.Email.Should().Be("juan@email.com");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "nonexistent@email.com",
            Password: "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("nonexistent@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "juan@email.com",
            Password: "WrongPassword!");

        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hashed_password");
        user.VerifyEmail();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("juan@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("WrongPassword!", "hashed_password"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Login_WithLockedAccount_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "juan@email.com",
            Password: "Password123!");

        var user = User.Create(
            "Juan", 
            "Pérez", 
            "juan@email.com", 
            "12345678", 
            DocumentType.DNI, 
            "hashed_password");
        
        // Bloquear la cuenta
        for (int i = 0; i < 5; i++) user.RecordFailedLogin();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("juan@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountLocked");
    }

    #endregion
}
