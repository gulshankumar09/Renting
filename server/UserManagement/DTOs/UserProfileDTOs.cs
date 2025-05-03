using System.ComponentModel.DataAnnotations;
using UserManagement.Models;
using UserManagement.Models.Entities;

namespace UserManagement.DTOs;

public class UserProfileDTO
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPhoneVerified { get; set; }
    public string ProfilePicture { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string PreferredLanguage { get; set; } = string.Empty;
}

public class UpdateUserProfileDTO
{
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(500)]
    public string Bio { get; set; } = string.Empty;

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    [StringLength(50)]
    public string PreferredLanguage { get; set; } = string.Empty;
}

public class UserPreferencesDTO
{
    public string Id { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public bool DarkMode { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
}

public class UpdateUserPreferencesDTO
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public bool DarkMode { get; set; }

    [StringLength(100)]
    public string TimeZone { get; set; } = string.Empty;

    [StringLength(100)]
    public string Theme { get; set; } = string.Empty;
}