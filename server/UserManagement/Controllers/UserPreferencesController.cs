using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/v1/preferences")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;
    private readonly IUserProfileService _userProfileService;

    public UserPreferencesController(
        IUserPreferencesService preferencesService,
        IUserProfileService userProfileService)
    {
        _preferencesService = preferencesService;
        _userProfileService = userProfileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyPreferences()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var userProfile = await _userProfileService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound(new { Error = "User profile not found." });
        }

        var preferences = await _preferencesService.GetUserPreferencesAsync(userProfile.Id);
        return Ok(preferences);
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMyPreferences([FromBody] UpdatePreferencesDto updateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var userProfile = await _userProfileService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound(new { Error = "User profile not found." });
        }

        var updatedPreferences = await _preferencesService.UpdateUserPreferencesAsync(userProfile.Id, updateDto);
        return Ok(updatedPreferences);
    }

    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetMyPreferences()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var userProfile = await _userProfileService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound(new { Error = "User profile not found." });
        }

        await _preferencesService.ResetUserPreferencesToDefaultAsync(userProfile.Id);
        return NoContent();
    }

    [HttpGet("users/{userProfileId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPreferences(string userProfileId)
    {
        var preferences = await _preferencesService.GetUserPreferencesAsync(userProfileId);
        return Ok(preferences);
    }

    [HttpPut("users/{userProfileId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserPreferences(string userProfileId, [FromBody] UpdatePreferencesDto updateDto)
    {
        var updatedPreferences = await _preferencesService.UpdateUserPreferencesAsync(userProfileId, updateDto);
        return Ok(updatedPreferences);
    }

    [HttpPost("users/{userProfileId}/reset")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetUserPreferences(string userProfileId)
    {
        await _preferencesService.ResetUserPreferencesToDefaultAsync(userProfileId);
        return NoContent();
    }
}