using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;
using UserManagement.Models.Exceptions;

namespace UserManagement.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(UserManager<ApplicationUser> userManager, 
    IConfiguration configuration, 
    ILogger<AuthService> logger,
    JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AuthException("User with this email already exists", "EMAIL_EXISTS");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new AuthException(
                string.Join(", ", result.Errors.Select(e => e.Description)),
                "REGISTRATION_FAILED"
            );

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
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

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        return _jwtTokenService.GetPrincipalFromExpiredToken(token);
    }
}