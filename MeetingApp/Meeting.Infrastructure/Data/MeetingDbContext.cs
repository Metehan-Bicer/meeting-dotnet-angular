using Meeting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meeting.Infrastructure.Data
{
    public class MeetingDbContext : DbContext
    {
        public MeetingDbContext(DbContextOptions<MeetingDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MeetingEntity> Meetings { get; set; }
        public DbSet<MeetingDeleteLog> MeetingDeleteLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Configure Meeting entity
            modelBuilder.Entity<MeetingEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                
                // Configure relationship
                entity.HasOne(m => m.User)
                      .WithMany()
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MeetingDeleteLog entity
            modelBuilder.Entity<MeetingDeleteLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MeetingId).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DeletedAt).IsRequired();
                entity.Property(e => e.DeletedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DeleteReason).HasMaxLength(200);
                entity.Property(e => e.OriginalCreatedAt).IsRequired();
            });
        }

        // Configure SQLite-specific options
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default to in-memory database if not configured
                optionsBuilder.UseInMemoryDatabase("MeetingAppDb");
            }
        }
    }
}