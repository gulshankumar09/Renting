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

    // Email Verification Endpoints
    [HttpPost("verify-email")]
    public async Task<IResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDto)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(verifyEmailDto);
            if (result)
                return Results.Ok(new { message = "Email verified successfully" });

            return Results.BadRequest(new { message = "Email verification failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return Results.StatusCode(500);
        }
    }

    [HttpPost("resend-verification-email")]
    public async Task<IResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDTO resendVerificationEmailDto)
    {
        try
        {
            var result = await _authService.ResendVerificationEmailAsync(resendVerificationEmailDto);
            // Always return success for security reasons
            return Results.Ok(new { message = "If the email exists and is not verified, a verification email has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            return Results.StatusCode(500);
        }
    }

    // Password Reset Endpoints
    [HttpPost("forgot-password")]
    public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
    {
        try
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            // Always return success for security reasons
            return Results.Ok(new { message = "If the email exists in our system, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return Results.StatusCode(500);
        }
    }

    [HttpPost("reset-password")]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (result.Succeeded)
                return Results.Ok(new { message = "Password has been reset successfully" });

            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return Results.StatusCode(500);
        }
    }
}