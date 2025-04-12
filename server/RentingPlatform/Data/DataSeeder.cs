using Microsoft.EntityFrameworkCore;
using RentingPlatform.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RentingPlatform.Data
{
    public class DataSeeder
    {
        public static async Task SeedDefaultAdminAsync(ApplicationDbContext context)
        {
            const string adminEmail = "admin@rentingplatform.com";

            // Check if admin user exists
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                // Create new admin user if none exists
                adminUser = new User
                {
                    Name = "Platform Admin",
                    Email = adminEmail,
                    PasswordHash = HashPassword("Admin@123"),
                    PhoneNumber = "1234567890",
                    ProfilePictureUrl = "/images/default-admin.png",
                    IsAdmin = true,
                    IsHost = true,  // Admin is also a host by default
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                Console.WriteLine($"Created default admin user with email: {adminEmail}");
            }
            else if (!adminUser.IsAdmin)
            {
                // Update existing user to admin if needed
                adminUser.IsAdmin = true;
                adminUser.UpdatedAt = DateTime.UtcNow;

                context.Users.Update(adminUser);
                await context.SaveChangesAsync();

                Console.WriteLine($"Updated existing user to admin role: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Admin user already exists: {adminEmail}");
            }
        }

        // Simple password hashing method
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}