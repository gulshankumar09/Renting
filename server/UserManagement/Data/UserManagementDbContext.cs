using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data
{
    public class UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<UserDocument> UserDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
}