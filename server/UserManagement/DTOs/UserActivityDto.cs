using System;
using UserManagement.Models;

namespace UserManagement.DTOs;

public class UserActivityResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class ActivityFilterDto
{
    public string? UserId { get; set; }
    public string? ActivityType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccessful { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UserActivitySummaryDto
{
    public int TotalActivities { get; set; }
    public int SuccessfulActivities { get; set; }
    public int FailedActivities { get; set; }
    public int LoginCount { get; set; }
    public int FailedLoginCount { get; set; }
    public int ProfileUpdateCount { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
}

public class CreateActivityDto
{
    public string UserId { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; } = true;
    public string? AdditionalInfo { get; set; }
}