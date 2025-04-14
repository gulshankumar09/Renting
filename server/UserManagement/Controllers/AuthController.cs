using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Models.Exceptions;
using UserManagement.Services;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginRequestDTO request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (AuthException ex)
        {
            _logger.LogWarning("Authentication failed: {Message}", ex.Message);
            return Results.Problem(
                title: "Authentication Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?>
                {
                    { "errorCode", ex.ErrorCode }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during login");
            return Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequestDTO request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Results.Created($"/api/users/{response}", response);
        }
        catch (AuthException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return Results.Problem(
                title: "Registration Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    { "errorCode", ex.ErrorCode }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration");
            return Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Results.Ok(response);
        }
        catch (AuthException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Results.Problem(
                title: "Token Refresh Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?>
                {
                    { "errorCode", ex.ErrorCode }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token refresh");
            return Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (result.Succeeded)
                return Results.Ok();

            return Results.BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
        catch (AuthException ex)
        {
            _logger.LogWarning("Password change failed: {Message}", ex.Message);
            return Results.Problem(
                title: "Password Change Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    { "errorCode", ex.ErrorCode }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during password change");
            return Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IResult> RevokeToken()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await _authService.RevokeTokenAsync(userId);
            return result ? Results.Ok() : Results.NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token revocation");
            return Results.Problem(
                title: "Internal Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}