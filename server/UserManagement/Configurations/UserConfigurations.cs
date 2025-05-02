
using Microsoft.EntityFrameworkCore;
using UserManagement.Models.Entities;

namespace UserManagement.Data.Configurations;

public sealed class UserConfigurations
{
    public static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        ConfigureUserProfile(modelBuilder);
        ConfigureApplicationUser(modelBuilder);
    }

    private static void ConfigureUserProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>()
            .HasOne(u => u.Preferences)
            .WithOne(p => p.UserProfile)
            .HasForeignKey<UserPreferences>(p => p.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfile>()
            .HasMany(u => u.Documents)
            .WithOne(d => d.UserProfile)
            .HasForeignKey(d => d.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfile>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
    }

    private static void ConfigureApplicationUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
    }
}
