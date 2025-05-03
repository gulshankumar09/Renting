using Microsoft.AspNetCore.Identity;
using UserManagement.DTOs;

namespace UserManagement.Interfaces;

public interface IRoleService
{
    Task<List<RoleDTO>> GetAllRolesAsync();
    Task<RoleDTO> GetRoleByIdAsync(string roleId);
    Task<RoleDTO> CreateRoleAsync(CreateRoleDTO createRoleDto);
    Task<RoleDTO> UpdateRoleAsync(string roleId, UpdateRoleDTO updateRoleDto);
    Task<bool> DeleteRoleAsync(string roleId);
    Task<List<UserRoleDTO>> GetUsersInRoleAsync(string roleName);
    Task<IdentityResult> AssignRoleToUserAsync(AssignRoleDTO assignRoleDto);
    Task<IdentityResult> RemoveRoleFromUserAsync(AssignRoleDTO assignRoleDto);
    Task<UserRoleDTO> GetUserRolesAsync(string userId);
}