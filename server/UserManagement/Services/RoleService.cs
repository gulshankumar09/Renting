using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;

namespace UserManagement.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<RoleDTO>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Select(r => new RoleDTO { Id = r.Id, Name = r.Name ?? string.Empty }).ToList();
    }

    public async Task<RoleDTO> GetRoleByIdAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        return new RoleDTO { Id = role.Id, Name = role.Name ?? string.Empty };
    }

    public async Task<RoleDTO> CreateRoleAsync(CreateRoleDTO createRoleDto)
    {
        var roleExists = await _roleManager.RoleExistsAsync(createRoleDto.Name);
        if (roleExists)
        {
            throw new InvalidOperationException($"Role {createRoleDto.Name} already exists");
        }

        var role = new IdentityRole(createRoleDto.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role: {errors}");
        }

        return new RoleDTO { Id = role.Id, Name = role.Name ?? string.Empty };
    }

    public async Task<RoleDTO> UpdateRoleAsync(string roleId, UpdateRoleDTO updateRoleDto)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        role.Name = updateRoleDto.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update role: {errors}");
        }

        return new RoleDTO { Id = role.Id, Name = role.Name ?? string.Empty };
    }

    public async Task<bool> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return false;
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
        if (usersInRole.Any())
        {
            throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it has users assigned to it");
        }

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

    public async Task<List<UserRoleDTO>> GetUsersInRoleAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            throw new KeyNotFoundException($"Role {roleName} not found");
        }

        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var userRoleDtos = new List<UserRoleDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoleDtos.Add(new UserRoleDTO
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return userRoleDtos;
    }

    public async Task<IdentityResult> AssignRoleToUserAsync(AssignRoleDTO assignRoleDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {assignRoleDto.UserId} not found");
        }

        var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.RoleName);
        if (!roleExists)
        {
            throw new KeyNotFoundException($"Role {assignRoleDto.RoleName} not found");
        }

        if (await _userManager.IsInRoleAsync(user, assignRoleDto.RoleName))
        {
            return IdentityResult.Success;
        }

        return await _userManager.AddToRoleAsync(user, assignRoleDto.RoleName);
    }

    public async Task<IdentityResult> RemoveRoleFromUserAsync(AssignRoleDTO assignRoleDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {assignRoleDto.UserId} not found");
        }

        var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.RoleName);
        if (!roleExists)
        {
            throw new KeyNotFoundException($"Role {assignRoleDto.RoleName} not found");
        }

        if (!await _userManager.IsInRoleAsync(user, assignRoleDto.RoleName))
        {
            return IdentityResult.Success;
        }

        return await _userManager.RemoveFromRoleAsync(user, assignRoleDto.RoleName);
    }

    public async Task<UserRoleDTO> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new UserRoleDTO
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        };
    }
}