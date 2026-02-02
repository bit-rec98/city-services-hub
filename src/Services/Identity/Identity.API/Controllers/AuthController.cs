using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Application.Queries;

namespace Identity.API.Controllers;

/// <summary>
/// Controller para autenticación de usuarios.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DocumentNumber,
            (int)request.DocumentType,
            request.Password,
            request.ConfirmPassword);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "User.EmailExists" => Conflict(new { message = result.Error.Message }),
                "User.DocumentExists" => Conflict(new { message = result.Error.Message }),
                _ => BadRequest(new { message = result.Error.Message })
            };
        }

        _logger.LogInformation("Usuario registrado: {Email}", request.Email);
        return CreatedAtAction(nameof(GetCurrentUser), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Inicia sesión y retorna tokens de autenticación.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var command = new LoginCommand(request.Email, request.Password, request.TwoFactorCode);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Auth.InvalidCredentials" => Unauthorized(new { message = result.Error.Message }),
                "Auth.AccountLocked" => Unauthorized(new { message = result.Error.Message }),
                "Auth.AccountInactive" => Unauthorized(new { message = result.Error.Message }),
                "Auth.TwoFactorRequired" => Ok(new { requiresTwoFactor = true, message = result.Error.Message }),
                _ => BadRequest(new { message = result.Error.Message })
            };
        }

        _logger.LogInformation("Usuario autenticado: {Email}", request.Email);
        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene el perfil del usuario actual.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized();

        var query = new GetCurrentUserQuery(userId.Value);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene un usuario por ID (solo administradores).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator,SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { message = result.Error.Message });

        return Ok(result.Value);
    }

    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
            ?? User.FindFirst("sub");
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }
}
