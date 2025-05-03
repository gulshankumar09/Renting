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

    // Email Verification
    Task<bool> VerifyEmailAsync(VerifyEmailDTO verifyEmailDto);
    Task<bool> ResendVerificationEmailAsync(ResendVerificationEmailDTO resendVerificationEmailDto);

    // Password Reset
    Task<bool> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);
}