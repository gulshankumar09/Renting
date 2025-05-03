using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public class VerifyEmailDTO
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationEmailDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}