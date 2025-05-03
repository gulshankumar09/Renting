using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers;

[Authorize(Roles = "Admin")]
public class RoleController : BaseApiController
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(string id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Role not found with ID {RoleId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the role" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO createRoleDto)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(createRoleDto);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create role");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { message = "An error occurred while creating the role" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDTO updateRoleDto)
    {
        try
        {
            var role = await _roleService.UpdateRoleAsync(id, updateRoleDto);
            return Ok(role);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Role not found with ID {RoleId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update role with ID {RoleId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the role" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (result)
                return NoContent();
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete role with ID {RoleId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the role" });
        }
    }

    [HttpGet("users-in-role/{roleName}")]
    public async Task<IActionResult> GetUsersInRole(string roleName)
    {
        try
        {
            var users = await _roleService.GetUsersInRoleAsync(roleName);
            return Ok(users);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Role not found with name {RoleName}", roleName);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users in role {RoleName}", roleName);
            return StatusCode(500, new { message = "An error occurred while retrieving users in role" });
        }
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO assignRoleDto)
    {
        try
        {
            var result = await _roleService.AssignRoleToUserAsync(assignRoleDto);
            if (result.Succeeded)
                return Ok(new { message = $"Role {assignRoleDto.RoleName} assigned to user successfully" });

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User or role not found for assignment");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return StatusCode(500, new { message = "An error occurred while assigning role to user" });
        }
    }

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDTO assignRoleDto)
    {
        try
        {
            var result = await _roleService.RemoveRoleFromUserAsync(assignRoleDto);
            if (result.Succeeded)
                return Ok(new { message = $"Role {assignRoleDto.RoleName} removed from user successfully" });

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User or role not found for removal");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user");
            return StatusCode(500, new { message = "An error occurred while removing role from user" });
        }
    }

    [HttpGet("user-roles/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        // Allow users to view their own roles, but only admins can view others' roles
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            var userRoles = await _roleService.GetUserRolesAsync(userId);
            return Ok(userRoles);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found with ID {UserId}", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user with ID {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user roles" });
        }
    }
}