using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Interfaces;
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
        var response = await _authService.LoginAsync(request);
        return Results.Ok(response);
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterRequestDTO request)
    {
        var response = await _authService.RegisterAsync(request);
        return Results.Created($"/api/users/{response}", response);
    }

    [HttpPost("refresh-token")]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Results.Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
            return Results.Ok();

        return Results.BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IResult> RevokeToken()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var result = await _authService.RevokeTokenAsync(userId);
        return result ? Results.Ok() : Results.NotFound();
    }
}