using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public record LoginRequestDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;

    [Required]
    public string Password { get; init; } = default!;
}

public record RegisterRequestDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = default!;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = default!;

    [Required]
    public string FirstName { get; init; } = default!;

    [Required]
    public string LastName { get; init; } = default!;
}

public record AuthResponseDTO
{
    public string Token { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
    public DateTime Expiration { get; init; }
}

public record RefreshTokenRequestDTO
{
    [Required]
    public string Token { get; init; } = default!;

    [Required]
    public string RefreshToken { get; init; } = default!;
}

public record ChangePasswordRequestDTO
{
    [Required]
    public string CurrentPassword { get; init; } = default!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; init; } = default!;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; init; } = default!;
}