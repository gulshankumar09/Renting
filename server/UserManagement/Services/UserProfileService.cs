using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models;
using UserManagement.Models.Entities;

namespace UserManagement.Services;

public class UserProfileService : IUserProfileService
{
    private readonly UserManagementDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        UserManagementDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<UserProfileService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        var profile = await _dbContext.UserProfiles
            .Include(p => p.ApplicationUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            // Create a new profile if it doesn't exist
            profile = new UserProfile
            {
                ApplicationUserId = userId,
                Id = Guid.NewGuid().ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Status = UserStatus.Active
            };

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();
        }

        return new UserProfileDTO
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber,
            IsPhoneVerified = profile.IsPhoneVerified,
            ProfilePicture = profile.ProfilePicture,
            Status = profile.Status,
            Bio = profile.Bio,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            Country = profile.Country,
            PostalCode = profile.PostalCode,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            PreferredLanguage = profile.PreferredLanguage
        };
    }

    public async Task<UserProfileDTO> UpdateUserProfileAsync(string userId, UpdateUserProfileDTO updateUserProfileDto)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID {userId} not found");
        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId) ?? throw new KeyNotFoundException($"Profile for user with ID {userId} not found");

        // Update ApplicationUser properties
        user.FirstName = updateUserProfileDto.FirstName;
        user.LastName = updateUserProfileDto.LastName;
        user.PhoneNumber = updateUserProfileDto.PhoneNumber;

        // Update UserProfile properties
        profile.FirstName = updateUserProfileDto.FirstName;
        profile.LastName = updateUserProfileDto.LastName;
        profile.PhoneNumber = updateUserProfileDto.PhoneNumber;
        profile.Bio = updateUserProfileDto.Bio;
        profile.Address = updateUserProfileDto.Address;
        profile.City = updateUserProfileDto.City;
        profile.State = updateUserProfileDto.State;
        profile.Country = updateUserProfileDto.Country;
        profile.PostalCode = updateUserProfileDto.PostalCode;
        profile.DateOfBirth = updateUserProfileDto.DateOfBirth;
        profile.Gender = updateUserProfileDto.Gender;
        profile.PreferredLanguage = updateUserProfileDto.PreferredLanguage;

        await _userManager.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        return await GetUserProfileAsync(userId);
    }

    public async Task<bool> UpdateProfilePictureAsync(string userId, string profilePictureUrl)
    {
        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId) ?? throw new KeyNotFoundException($"Profile for user with ID {userId} not found");
        profile.ProfilePicture = profilePictureUrl;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<UserPreferencesDTO> GetUserPreferencesAsync(string userId)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId) ?? throw new KeyNotFoundException($"Profile for user with ID {userId} not found");
            
        if (profile.Preferences == null)
        {
            // Create default preferences if they don't exist
            profile.Preferences = new UserPreferences
            {
                Id = Guid.NewGuid().ToString(),
                UserProfileId = profile.Id,
                EmailNotifications = true,
                PushNotifications = false,
                SMSNotifications = false,
                MarketingEmails = false,
                NewsletterSubscription = false,
                TimeZone = "UTC",
                DarkMode = false
            };

            await _dbContext.SaveChangesAsync();
        }

        return new UserPreferencesDTO
        {
            Id = profile.Preferences.Id,
            EmailNotifications = profile.Preferences.EmailNotifications,
            SmsNotifications = profile.Preferences.SMSNotifications,
            MarketingEmails = profile.Preferences.MarketingEmails,
            DarkMode = profile.Preferences.DarkMode,
            TimeZone = profile.Preferences.TimeZone
        };
    }

    public async Task<UserPreferencesDTO> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesDTO preferencesDto)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId) ?? throw new KeyNotFoundException($"Profile for user with ID {userId} not found");
        
        profile.Preferences ??= new UserPreferences
            {
                Id = Guid.NewGuid().ToString(),
                UserProfileId = profile.Id
            };

        profile.Preferences.EmailNotifications = preferencesDto.EmailNotifications;
        profile.Preferences.MarketingEmails = preferencesDto.MarketingEmails;
        profile.Preferences.DarkMode = preferencesDto.DarkMode;
        profile.Preferences.TimeZone = preferencesDto.TimeZone;

        await _dbContext.SaveChangesAsync();

        return await GetUserPreferencesAsync(userId);
    }

    public async Task<bool> DeleteUserProfileAsync(string userId)
    {
        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            return false;
        }

        _dbContext.UserProfiles.Remove(profile);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}