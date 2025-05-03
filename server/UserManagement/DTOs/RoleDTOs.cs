using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public class RoleDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateRoleDTO
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateRoleDTO
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;
}

public class UserRoleDTO
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
}

public class AssignRoleDTO
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;
}