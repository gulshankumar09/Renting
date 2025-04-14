using Microsoft.AspNetCore.Identity;
using UserManagement.DTOs;

namespace UserManagement.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
    Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
    Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request);
    Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> RevokeTokenAsync(string userId);
}