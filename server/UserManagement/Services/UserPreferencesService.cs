using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;

namespace UserManagement.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly UserManagementDbContext _context;
    private readonly IUserActivityService _activityService;

    public UserPreferencesService(
        UserManagementDbContext context,
        IUserActivityService activityService)
    {
        _context = context;
        _activityService = activityService;
    }

    public async Task<UserPreferencesDto> GetUserPreferencesAsync(string userProfileId)
    {
        var preferences = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

        if (preferences == null)
        {
            // If preferences don't exist yet, create default preferences
            preferences = await CreateDefaultPreferencesAsync(userProfileId);
        }

        return MapToUserPreferencesDto(preferences);
    }

    public async Task<UserPreferencesDto> UpdateUserPreferencesAsync(string userProfileId, UpdatePreferencesDto updateDto)
    {
        var preferences = await _context.UserPreferences
            .Include(p => p.UserProfile)
            .ThenInclude(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

        if (preferences == null)
        {
            // If preferences don't exist yet, create default preferences
            preferences = await CreateDefaultPreferencesAsync(userProfileId);
        }

        // Update properties if provided
        if (updateDto.DarkMode.HasValue)
            preferences.DarkMode = updateDto.DarkMode.Value;

        // Communication Preferences
        if (updateDto.EmailNotifications.HasValue)
            preferences.EmailNotifications = updateDto.EmailNotifications.Value;
        if (updateDto.PushNotifications.HasValue)
            preferences.PushNotifications = updateDto.PushNotifications.Value;
        if (updateDto.SMSNotifications.HasValue)
            preferences.SMSNotifications = updateDto.SMSNotifications.Value;
        if (updateDto.MarketingEmails.HasValue)
            preferences.MarketingEmails = updateDto.MarketingEmails.Value;
        if (updateDto.NewsletterSubscription.HasValue)
            preferences.NewsletterSubscription = updateDto.NewsletterSubscription.Value;

        // Language and Regional Settings
        if (!string.IsNullOrEmpty(updateDto.Language))
            preferences.Language = updateDto.Language;
        if (!string.IsNullOrEmpty(updateDto.TimeZone))
            preferences.TimeZone = updateDto.TimeZone;
        if (!string.IsNullOrEmpty(updateDto.Currency))
            preferences.Currency = updateDto.Currency;
        if (!string.IsNullOrEmpty(updateDto.DateFormat))
            preferences.DateFormat = updateDto.DateFormat;

        // Property Preferences
        if (updateDto.PreferredPropertyTypes != null)
            preferences.PreferredPropertyTypes = updateDto.PreferredPropertyTypes;
        if (updateDto.MinPriceRange.HasValue)
            preferences.MinPriceRange = updateDto.MinPriceRange;
        if (updateDto.MaxPriceRange.HasValue)
            preferences.MaxPriceRange = updateDto.MaxPriceRange;
        if (updateDto.PreferredLocations != null)
            preferences.PreferredLocations = updateDto.PreferredLocations;
        if (updateDto.PreferredAmenities != null)
            preferences.PreferredAmenities = updateDto.PreferredAmenities;
        if (updateDto.PreferredBedrooms.HasValue)
            preferences.PreferredBedrooms = updateDto.PreferredBedrooms;
        if (updateDto.PreferredBathrooms.HasValue)
            preferences.PreferredBathrooms = updateDto.PreferredBathrooms;

        // Search and Filter Preferences
        if (updateDto.ShowVerifiedPropertiesOnly.HasValue)
            preferences.ShowVerifiedPropertiesOnly = updateDto.ShowVerifiedPropertiesOnly.Value;
        if (updateDto.ShowInstantBookPropertiesOnly.HasValue)
            preferences.ShowInstantBookPropertiesOnly = updateDto.ShowInstantBookPropertiesOnly.Value;
        if (updateDto.ShowSuperhostPropertiesOnly.HasValue)
            preferences.ShowSuperhostPropertiesOnly = updateDto.ShowSuperhostPropertiesOnly.Value;
        if (!string.IsNullOrEmpty(updateDto.DefaultSortOrder))
            preferences.DefaultSortOrder = updateDto.DefaultSortOrder;
        if (updateDto.DefaultResultsPerPage.HasValue)
            preferences.DefaultResultsPerPage = updateDto.DefaultResultsPerPage.Value;

        // Privacy Settings
        if (updateDto.ShowProfileToPublic.HasValue)
            preferences.ShowProfileToPublic = updateDto.ShowProfileToPublic.Value;
        if (updateDto.ShowEmailToPublic.HasValue)
            preferences.ShowEmailToPublic = updateDto.ShowEmailToPublic.Value;
        if (updateDto.ShowPhoneToPublic.HasValue)
            preferences.ShowPhoneToPublic = updateDto.ShowPhoneToPublic.Value;
        if (updateDto.ShowReviewsToPublic.HasValue)
            preferences.ShowReviewsToPublic = updateDto.ShowReviewsToPublic.Value;

        // Notification Preferences
        if (updateDto.NotifyOnBookingRequests.HasValue)
            preferences.NotifyOnBookingRequests = updateDto.NotifyOnBookingRequests.Value;
        if (updateDto.NotifyOnBookingConfirmations.HasValue)
            preferences.NotifyOnBookingConfirmations = updateDto.NotifyOnBookingConfirmations.Value;
        if (updateDto.NotifyOnMessages.HasValue)
            preferences.NotifyOnMessages = updateDto.NotifyOnMessages.Value;
        if (updateDto.NotifyOnReviews.HasValue)
            preferences.NotifyOnReviews = updateDto.NotifyOnReviews.Value;
        if (updateDto.NotifyOnPriceChanges.HasValue)
            preferences.NotifyOnPriceChanges = updateDto.NotifyOnPriceChanges.Value;
        if (updateDto.NotifyOnNewProperties.HasValue)
            preferences.NotifyOnNewProperties = updateDto.NotifyOnNewProperties.Value;

        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log the preference update activity
        if (preferences.UserProfile?.ApplicationUser != null)
        {
            var userId = preferences.UserProfile.ApplicationUserId;
            await _activityService.LogGenericActivityAsync(
                userId,
                Models.ActivityType.PreferenceUpdate,
                "User preferences updated",
                "Internal", // IP address is not needed for internal activities
                "System", // User agent is not needed for internal activities
                true,
                null
            );
        }

        return MapToUserPreferencesDto(preferences);
    }

    public async Task<bool> ResetUserPreferencesToDefaultAsync(string userProfileId)
    {
        var preferences = await _context.UserPreferences
            .Include(p => p.UserProfile)
            .ThenInclude(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

        if (preferences != null)
        {
            _context.UserPreferences.Remove(preferences);
            await _context.SaveChangesAsync();
        }

        await CreateDefaultPreferencesAsync(userProfileId);

        // Log the preference reset activity
        var userProfile = await _context.UserProfiles
            .Include(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.Id == userProfileId);

        if (userProfile?.ApplicationUser != null)
        {
            var userId = userProfile.ApplicationUserId;
            await _activityService.LogGenericActivityAsync(
                userId,
                Models.ActivityType.PreferenceUpdate,
                "User preferences reset to default",
                "Internal", // IP address is not needed for internal activities
                "System", // User agent is not needed for internal activities
                true,
                null
            );
        }

        return true;
    }

    private async Task<UserPreferences> CreateDefaultPreferencesAsync(string userProfileId)
    {
        var preferences = new UserPreferences
        {
            Id = Guid.NewGuid().ToString(),
            UserProfileId = userProfileId,
            DarkMode = false,
            EmailNotifications = true,
            PushNotifications = true,
            SMSNotifications = false,
            MarketingEmails = false,
            NewsletterSubscription = true,
            Language = "en",
            TimeZone = "UTC",
            Currency = "USD",
            DateFormat = "MM/dd/yyyy",
            PreferredPropertyTypes = new List<string>(),
            MinPriceRange = null,
            MaxPriceRange = null,
            PreferredLocations = new List<string>(),
            PreferredAmenities = new List<string>(),
            PreferredBedrooms = null,
            PreferredBathrooms = null,
            ShowVerifiedPropertiesOnly = false,
            ShowInstantBookPropertiesOnly = false,
            ShowSuperhostPropertiesOnly = false,
            DefaultSortOrder = "relevance",
            DefaultResultsPerPage = 20,
            ShowProfileToPublic = true,
            ShowEmailToPublic = false,
            ShowPhoneToPublic = false,
            ShowReviewsToPublic = true,
            NotifyOnBookingRequests = true,
            NotifyOnBookingConfirmations = true,
            NotifyOnMessages = true,
            NotifyOnReviews = true,
            NotifyOnPriceChanges = false,
            NotifyOnNewProperties = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserPreferences.Add(preferences);
        await _context.SaveChangesAsync();

        return preferences;
    }

    private UserPreferencesDto MapToUserPreferencesDto(UserPreferences preferences)
    {
        return new UserPreferencesDto
        {
            DarkMode = preferences.DarkMode,
            EmailNotifications = preferences.EmailNotifications,
            PushNotifications = preferences.PushNotifications,
            SMSNotifications = preferences.SMSNotifications,
            MarketingEmails = preferences.MarketingEmails,
            NewsletterSubscription = preferences.NewsletterSubscription,
            Language = preferences.Language,
            TimeZone = preferences.TimeZone,
            Currency = preferences.Currency,
            DateFormat = preferences.DateFormat,
            PreferredPropertyTypes = preferences.PreferredPropertyTypes,
            MinPriceRange = preferences.MinPriceRange,
            MaxPriceRange = preferences.MaxPriceRange,
            PreferredLocations = preferences.PreferredLocations,
            PreferredAmenities = preferences.PreferredAmenities,
            PreferredBedrooms = preferences.PreferredBedrooms,
            PreferredBathrooms = preferences.PreferredBathrooms,
            ShowVerifiedPropertiesOnly = preferences.ShowVerifiedPropertiesOnly,
            ShowInstantBookPropertiesOnly = preferences.ShowInstantBookPropertiesOnly,
            ShowSuperhostPropertiesOnly = preferences.ShowSuperhostPropertiesOnly,
            DefaultSortOrder = preferences.DefaultSortOrder,
            DefaultResultsPerPage = preferences.DefaultResultsPerPage,
            ShowProfileToPublic = preferences.ShowProfileToPublic,
            ShowEmailToPublic = preferences.ShowEmailToPublic,
            ShowPhoneToPublic = preferences.ShowPhoneToPublic,
            ShowReviewsToPublic = preferences.ShowReviewsToPublic,
            NotifyOnBookingRequests = preferences.NotifyOnBookingRequests,
            NotifyOnBookingConfirmations = preferences.NotifyOnBookingConfirmations,
            NotifyOnMessages = preferences.NotifyOnMessages,
            NotifyOnReviews = preferences.NotifyOnReviews,
            NotifyOnPriceChanges = preferences.NotifyOnPriceChanges,
            NotifyOnNewProperties = preferences.NotifyOnNewProperties
        };
    }
}