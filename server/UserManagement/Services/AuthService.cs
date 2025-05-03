using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;
using UserManagement.Models.Exceptions;
using UserManagement.Models;

namespace UserManagement.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IUserActivityService _activityService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ILogger<AuthService> logger,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        IUserActivityService activityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _configuration = configuration;
        _activityService = activityService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        string ipAddress = GetIpAddress();
        string userAgent = GetUserAgent();

        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                await _activityService.LogLoginAttemptAsync(
                    null,
                    false,
                    ipAddress,
                    userAgent,
                    $"Failed login attempt: User with email {request.Email} not found");

                throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _activityService.LogLoginAttemptAsync(
                    user.Id,
                    false,
                    ipAddress,
                    userAgent,
                    "Failed login attempt: Invalid password");

                throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");
            }

            if (!user.EmailConfirmed && await _userManager.IsEmailConfirmedAsync(user))
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            if (!user.EmailConfirmed)
            {
                await _activityService.LogLoginAttemptAsync(
                    user.Id,
                    false,
                    ipAddress,
                    userAgent,
                    "Failed login attempt: Email not confirmed");

                throw new AuthException("Email not confirmed. Please check your email for verification link.", "EMAIL_NOT_VERIFIED");
            }

            // Log successful login
            await _activityService.LogLoginAttemptAsync(
                user.Id,
                true,
                ipAddress,
                userAgent);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return await GenerateAuthResponseAsync(user);
        }
        catch (AuthException)
        {
            // Rethrow auth exceptions as they're already logged
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
            throw new AuthException("An error occurred during login", "LOGIN_ERROR");
        }
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        string ipAddress = GetIpAddress();
        string userAgent = GetUserAgent();

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                await _activityService.LogGenericActivityAsync(
                    null,
                    ActivityType.Registration,
                    $"Registration failed: Email {request.Email} already exists",
                    ipAddress,
                    userAgent,
                    false);

                throw new AuthException("User with this email already exists", "EMAIL_EXISTS");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                await _activityService.LogGenericActivityAsync(
                    null,
                    ActivityType.Registration,
                    $"Registration failed: {errors}",
                    ipAddress,
                    userAgent,
                    false,
                    errors);

                throw new AuthException(errors, "REGISTRATION_FAILED");
            }

            // Log successful registration
            await _activityService.LogGenericActivityAsync(
                user.Id,
                ActivityType.Registration,
                "User registered successfully",
                ipAddress,
                userAgent,
                true);

            // Generate email confirmation token and send verification email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.Id, token);

            return await GenerateAuthResponseAsync(user);
        }
        catch (AuthException)
        {
            // Rethrow auth exceptions as they're already logged
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            throw new AuthException("An error occurred during registration", "REGISTRATION_ERROR");
        }
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            throw new AuthException("Invalid token", "INVALID_TOKEN");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new AuthException("Invalid refresh token", "INVALID_REFRESH_TOKEN");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new AuthException("User not found", "USER_NOT_FOUND");

        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<bool> RevokeTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // Email Verification
    public async Task<bool> VerifyEmailAsync(VerifyEmailDTO verifyEmailDto)
    {
        var user = await _userManager.FindByIdAsync(verifyEmailDto.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for email verification: {UserId}", verifyEmailDto.UserId);
            return false;
        }

        if (user.EmailConfirmed)
        {
            return true; // Email already verified
        }

        var result = await _userManager.ConfirmEmailAsync(user, verifyEmailDto.Token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);
            return true;
        }

        _logger.LogWarning("Email verification failed for user: {Email}, errors: {Errors}",
            user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        return false;
    }

    public async Task<bool> ResendVerificationEmailAsync(ResendVerificationEmailDTO resendVerificationEmailDto)
    {
        var user = await _userManager.FindByEmailAsync(resendVerificationEmailDto.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for resending verification email: {Email}", resendVerificationEmailDto.Email);
            return false;
        }

        if (user.EmailConfirmed)
        {
            return true; // Email already verified
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailVerificationAsync(user.Email!, user.Id, token);

        _logger.LogInformation("Verification email resent to: {Email}", user.Email);
        return true;
    }

    // Password Reset
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // We return true even if the user is not found for security reasons
            _logger.LogWarning("User not found for password reset: {Email}", forgotPasswordDto.Email);
            return true;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.Id, token);

        _logger.LogInformation("Password reset email sent to: {Email}", user.Email);
        return true;
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
    {
        var user = await _userManager.FindByIdAsync(resetPasswordDto.UserId);
        if (user == null)
        {
            _logger.LogWarning("User not found for password reset: {UserId}", resetPasswordDto.UserId);
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successful for user: {Email}", user.Email);

            // Revoke any existing refresh tokens for security
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
        else
        {
            _logger.LogWarning("Password reset failed for user: {Email}, errors: {Errors}",
                user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    private async Task<AuthResponseDTO> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var token = _jwtTokenService.GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return "Unknown";

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrEmpty(ipAddress) ? "Unknown" : ipAddress;
    }

    private string GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return "Unknown";

        var userAgent = context.Request.Headers["User-Agent"].ToString();
        return string.IsNullOrEmpty(userAgent) ? "Unknown" : userAgent;
    }
}