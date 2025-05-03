using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;
using UserManagement.Models.Entities;

namespace UserManagement.Services;

public class UserActivityService : IUserActivityService
{
    private readonly UserManagementDbContext _context;

    public UserActivityService(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<UserActivityResponseDto> CreateActivityAsync(CreateActivityDto activityDto)
    {
        var activity = new UserActivity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = activityDto.UserId,
            ActivityType = activityDto.ActivityType.ToString(),
            Description = activityDto.Description,
            IpAddress = activityDto.IpAddress,
            UserAgent = activityDto.UserAgent,
            IsSuccessful = activityDto.IsSuccessful,
            AdditionalInfo = activityDto.AdditionalInfo,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();

        return await MapToUserActivityResponseDto(activity);
    }

    public async Task<UserActivityResponseDto> GetActivityByIdAsync(string activityId)
    {
        var activity = await _context.UserActivities.FindAsync(activityId)
            ?? throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

        return await MapToUserActivityResponseDto(activity);
    }

    public async Task<(IEnumerable<UserActivityResponseDto> Activities, int TotalCount)> GetUserActivitiesAsync(string userId, ActivityFilterDto filterDto)
    {
        var query = _context.UserActivities
            .Where(a => a.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, filterDto);

        var totalCount = await query.CountAsync();

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filterDto.Page - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .ToListAsync();

        var activityDtos = new List<UserActivityResponseDto>();
        foreach (var activity in activities)
        {
            activityDtos.Add(await MapToUserActivityResponseDto(activity));
        }

        return (activityDtos, totalCount);
    }

    public async Task<(IEnumerable<UserActivityResponseDto> Activities, int TotalCount)> GetAllActivitiesAsync(ActivityFilterDto filterDto)
    {
        var query = _context.UserActivities.AsQueryable();

        query = ApplyFilters(query, filterDto);

        var totalCount = await query.CountAsync();

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filterDto.Page - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .ToListAsync();

        var activityDtos = new List<UserActivityResponseDto>();
        foreach (var activity in activities)
        {
            activityDtos.Add(await MapToUserActivityResponseDto(activity));
        }

        return (activityDtos, totalCount);
    }

    public async Task<UserActivitySummaryDto> GetUserActivitySummaryAsync(string userId)
    {
        var activities = await _context.UserActivities
            .Where(a => a.UserId == userId)
            .ToListAsync();

        if (!activities.Any())
        {
            return new UserActivitySummaryDto
            {
                TotalActivities = 0,
                SuccessfulActivities = 0,
                FailedActivities = 0,
                LoginCount = 0,
                FailedLoginCount = 0,
                ProfileUpdateCount = 0
            };
        }

        var loginActivities = activities.Where(a => a.ActivityType == ActivityType.Login.ToString()).ToList();
        var failedLoginActivities = activities.Where(a => a.ActivityType == ActivityType.FailedLogin.ToString()).ToList();
        var profileUpdateActivities = activities.Where(a => a.ActivityType == ActivityType.ProfileUpdate.ToString()).ToList();

        return new UserActivitySummaryDto
        {
            TotalActivities = activities.Count,
            SuccessfulActivities = activities.Count(a => a.IsSuccessful),
            FailedActivities = activities.Count(a => !a.IsSuccessful),
            LoginCount = loginActivities.Count,
            FailedLoginCount = failedLoginActivities.Count,
            ProfileUpdateCount = profileUpdateActivities.Count,
            LastLoginTime = loginActivities.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastActivityTime = activities.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt
        };
    }

    public async Task<bool> LogLoginAttemptAsync(string? userId, bool isSuccessful, string ipAddress, string userAgent, string? errorMessage = null)
    {
        var activityType = isSuccessful ? ActivityType.Login : ActivityType.FailedLogin;
        var description = isSuccessful ? "User logged in successfully" : "Failed login attempt";

        var activity = new UserActivity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ActivityType = activityType.ToString(),
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = isSuccessful,
            AdditionalInfo = errorMessage,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserActivities.Add(activity);

        // Update last login time if successful
        if (isSuccessful && !string.IsNullOrEmpty(userId))
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LogProfileUpdateAsync(string userId, string description, string ipAddress, string userAgent)
    {
        return await LogGenericActivityAsync(userId, ActivityType.ProfileUpdate, description, ipAddress, userAgent);
    }

    public async Task<bool> LogGenericActivityAsync(string? userId, ActivityType activityType, string description, string ipAddress, string userAgent, bool isSuccessful = true, string? additionalInfo = null)
    {
        var activity = new UserActivity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ActivityType = activityType.ToString(),
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = isSuccessful,
            AdditionalInfo = additionalInfo,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<UserActivity> ApplyFilters(IQueryable<UserActivity> query, ActivityFilterDto filterDto)
    {
        if (!string.IsNullOrEmpty(filterDto.UserId))
        {
            query = query.Where(a => a.UserId == filterDto.UserId);
        }

        if (!string.IsNullOrEmpty(filterDto.ActivityType))
        {
            query = query.Where(a => a.ActivityType == filterDto.ActivityType);
        }

        if (filterDto.StartDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= filterDto.StartDate);
        }

        if (filterDto.EndDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= filterDto.EndDate);
        }

        if (filterDto.IsSuccessful.HasValue)
        {
            query = query.Where(a => a.IsSuccessful == filterDto.IsSuccessful);
        }

        return query;
    }

    private async Task<UserActivityResponseDto> MapToUserActivityResponseDto(UserActivity activity)
    {
        string userEmail = "Unknown";

        if (!string.IsNullOrEmpty(activity.UserId))
        {
            var user = await _context.Users.FindAsync(activity.UserId);
            if (user != null)
            {
                userEmail = user.Email ?? "Unknown";
            }
        }

        return new UserActivityResponseDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            UserEmail = userEmail,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            IpAddress = activity.IpAddress,
            IsSuccessful = activity.IsSuccessful,
            CreatedAt = activity.CreatedAt,
            AdditionalInfo = activity.AdditionalInfo
        };
    }
}