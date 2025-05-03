using System.Threading.Tasks;
using UserManagement.DTOs;

namespace UserManagement.Interfaces;

public interface IUserPreferencesService
{
    Task<UserPreferencesDto> GetUserPreferencesAsync(string userProfileId);
    Task<UserPreferencesDto> UpdateUserPreferencesAsync(string userProfileId, UpdatePreferencesDto updateDto);
    Task<bool> ResetUserPreferencesToDefaultAsync(string userProfileId);
}