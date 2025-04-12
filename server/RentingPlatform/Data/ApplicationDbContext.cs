using Microsoft.EntityFrameworkCore;
using RentingPlatform.Models;

namespace RentingPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships to avoid multiple cascade paths

            // Product -> Booking (keep cascade)
            modelBuilder.Entity<Booking>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Booking (change to restrict)
            modelBuilder.Entity<Booking>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Product -> Review (keep cascade)
            modelBuilder.Entity<Review>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Review (change to restrict)
            modelBuilder.Entity<Review>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> Product (keep cascade)
            modelBuilder.Entity<Product>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.HostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}