using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;
using UserManagement.Models.Entities;
using UserManagement.Models.Exceptions;

namespace UserManagement.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new AuthException("Invalid email or password", "INVALID_CREDENTIALS");

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
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDTO
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(15)
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new AuthException("JWT Secret not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(15);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["JWT:ValidIssuer"],
            ValidAudience = _configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new AuthException("JWT Secret not configured")))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new AuthException("Invalid token", "INVALID_TOKEN");
        }

        return principal;
    }
}