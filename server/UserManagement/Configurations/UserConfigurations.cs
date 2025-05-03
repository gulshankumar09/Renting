using Microsoft.EntityFrameworkCore;
using UserManagement.Models.Entities;

namespace UserManagement.Data.Configurations;

public sealed class UserConfigurations
{
    public static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        // Configure entities that inherit from BaseEntity to have string Id (not identity)
        modelBuilder.Entity<UserProfile>()
            .Property(e => e.Id)
            .HasColumnType("text")
            .ValueGeneratedNever();

        modelBuilder.Entity<UserPreferences>()
            .Property(e => e.Id)
            .HasColumnType("text")
            .ValueGeneratedNever();

        modelBuilder.Entity<UserDocument>()
            .Property(e => e.Id)
            .HasColumnType("text")
            .ValueGeneratedNever();

        modelBuilder.Entity<UserActivity>()
            .Property(e => e.Id)
            .HasColumnType("text")
            .ValueGeneratedNever();

        // Configure UserProfile relationship with ApplicationUser
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<UserProfile>(p => p.ApplicationUserId);

        // Configure UserProfile
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

        // Configure UserActivity relationship with ApplicationUser
        modelBuilder.Entity<UserActivity>()
            .HasOne(a => a.User)
            .WithMany(u => u.Activities)
            .HasForeignKey(a => a.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure indexes
        modelBuilder.Entity<UserProfile>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // Configure Identity
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
    }
}
