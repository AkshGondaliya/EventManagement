using EventManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Notification> Notifications { get; set; }


        // ADD THIS ENTIRE METHOD
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // An Event has one Creator (User)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany() // A User can have many Events
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); // <-- This is the key change

            // A Registration has one User
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // This can stay as cascade

            // A Registration has one Event
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade); // This can stay as cascade

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 100,
                FullName = "System Admin",
                Email = "admin@college.edu",
                PasswordHash = adminPasswordHash,
                Role = "Admin"
            });

        }

    }
}
