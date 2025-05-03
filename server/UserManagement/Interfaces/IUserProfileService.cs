using UserManagement.DTOs;
using UserManagement.Models.Entities;

namespace UserManagement.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDTO> GetUserProfileAsync(string userId);
    Task<UserProfileDTO> UpdateUserProfileAsync(string userId, UpdateUserProfileDTO updateUserProfileDto);
    Task<bool> UpdateProfilePictureAsync(string userId, string profilePictureUrl);
    Task<UserPreferencesDTO> GetUserPreferencesAsync(string userId);
    Task<UserPreferencesDTO> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesDTO preferencesDto);
    Task<bool> DeleteUserProfileAsync(string userId);
}