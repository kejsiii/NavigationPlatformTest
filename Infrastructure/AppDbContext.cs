using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<JourneyShare> JourneyShares { get; set; }
        public DbSet<JourneyPublicLink> JourneyPublicLinks { get; set; }
        public DbSet<DailyGoalBadge> DailyGoalBadges { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserStatusChange> UserStatusChanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role - User
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Journey
            modelBuilder.Entity<User>()
                .HasMany(u => u.Journeys)
                .WithOne(j => j.User)
                .HasForeignKey(j => j.UserId);

            // User - DailyGoalBadges
            modelBuilder.Entity<User>()
                .HasMany(u => u.DailyGoalBadges)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId);

            // Journey - JourneyPublicLinks
            modelBuilder.Entity<Journey>()
                .HasMany(j => j.JourneyPublicLinks)
                .WithOne(jp => jp.Journey)
                .HasForeignKey(jp => jp.JourneyId);

            // User - JourneyShares (shared by user)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SharedJourneys)
                .WithOne(js => js.SharingUser)
                .HasForeignKey(js => js.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - JourneyShares (received by user)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedJourneys)
                .WithOne(js => js.RecievingUser)
                .HasForeignKey(js => js.RecievingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Journey - JourneyShares
            modelBuilder.Entity<Journey>()
                .HasMany(j => j.JourneyShares)
                .WithOne(js => js.Journey)
                .HasForeignKey(js => js.JourneyId);

            // User - AuditLogs
            modelBuilder.Entity<User>()
                .HasMany(u => u.AuditLog)
                .WithOne(al => al.User)
                .HasForeignKey(al => al.UserId);

            // User - UserStatusChange (target)
            modelBuilder.Entity<User>()
                .HasMany(u => u.StatusChanges)
                .WithOne(usc => usc.User)
                .HasForeignKey(usc => usc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - UserStatusChange (admin)
            modelBuilder.Entity<User>()
                .HasMany(u => u.PerformedStatusChanges)
                .WithOne(usc => usc.ChangedByAdmin)
                .HasForeignKey(usc => usc.ChangedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);


            //  Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-000000000001"),
                    Name = "User"
                },
                new Role
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-000000000002"),
                    Name = "Admin"
                }
            );
        }
    }
}

