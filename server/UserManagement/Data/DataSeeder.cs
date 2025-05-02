using Microsoft.AspNetCore.Identity;
using UserManagement.Models;
using UserManagement.Models.Entities;

namespace UserManagement.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Enum.GetNames<Role>())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
    {
        // Check if admin user already exists
        var adminUser = await userManager.FindByEmailAsync("admin@renting.com");
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@renting.com",
                Email = "admin@renting.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Role.Admin.ToString());
            }
        }
    }
}