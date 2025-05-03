using System;

namespace UserManagement.Models.Entities;

public class UserActivity : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? AdditionalInfo { get; set; }

    // Navigation property
    public virtual ApplicationUser User { get; set; } = null!;
}