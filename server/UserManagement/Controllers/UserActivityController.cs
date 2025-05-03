using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers;

[Route("api/v1/activities")]
[Authorize]
public class UserActivityController : BaseApiController
{
    private readonly IUserActivityService _activityService;

    public UserActivityController(IUserActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllActivities([FromQuery] ActivityFilterDto filterDto)
    {
        var (activities, totalCount) = await _activityService.GetAllActivitiesAsync(filterDto);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());

        return Ok(activities);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyActivities([FromQuery] ActivityFilterDto filterDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var (activities, totalCount) = await _activityService.GetUserActivitiesAsync(userId, filterDto);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());

        return Ok(activities);
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserActivities(string userId, [FromQuery] ActivityFilterDto filterDto)
    {
        var (activities, totalCount) = await _activityService.GetUserActivitiesAsync(userId, filterDto);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());

        return Ok(activities);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserActivityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityById(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        try
        {
            var activity = await _activityService.GetActivityByIdAsync(id);

            // Only admins or the user who owns the activity can view it
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && activity.UserId != userId)
            {
                return Forbid();
            }

            return Ok(activity);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Activity not found." });
        }
    }

    [HttpGet("summary/me")]
    [ProducesResponseType(typeof(UserActivitySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyActivitySummary()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var summary = await _activityService.GetUserActivitySummaryAsync(userId);
        return Ok(summary);
    }

    [HttpGet("summary/users/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserActivitySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserActivitySummary(string userId)
    {
        var summary = await _activityService.GetUserActivitySummaryAsync(userId);
        return Ok(summary);
    }
}