using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models.Entities;

namespace UserManagement.Data;

public class UserManagementDbContext : IdentityDbContext<ApplicationUser>
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserDocument> UserDocuments { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Activities)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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
