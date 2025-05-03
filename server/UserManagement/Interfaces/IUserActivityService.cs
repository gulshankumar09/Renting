using UserManagement.DTOs;
using UserManagement.Models;

namespace UserManagement.Interfaces;

public interface IUserActivityService
{
    Task<UserActivityResponseDto> CreateActivityAsync(CreateActivityDto activityDto);
    Task<UserActivityResponseDto> GetActivityByIdAsync(string activityId);
    Task<(IEnumerable<UserActivityResponseDto> Activities, int TotalCount)> GetUserActivitiesAsync(string userId, ActivityFilterDto filterDto);
    Task<(IEnumerable<UserActivityResponseDto> Activities, int TotalCount)> GetAllActivitiesAsync(ActivityFilterDto filterDto);
    Task<UserActivitySummaryDto> GetUserActivitySummaryAsync(string userId);
    Task<bool> LogLoginAttemptAsync(string? userId, bool isSuccessful, string ipAddress, string userAgent, string? errorMessage = null);
    Task<bool> LogProfileUpdateAsync(string userId, string description, string ipAddress, string userAgent);
    Task<bool> LogGenericActivityAsync(string? userId, ActivityType activityType, string description, string ipAddress, string userAgent, bool isSuccessful = true, string? additionalInfo = null);
}