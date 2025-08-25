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
        public DbSet<FileMetadata> FileMetadata { get; set; }

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
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ProfileImagePath).HasMaxLength(500);
            });

            // Configure Meeting entity
            modelBuilder.Entity<MeetingEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.DocumentPath).HasMaxLength(500);
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

            // Configure FileMetadata entity
            modelBuilder.Entity<FileMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileExtension).IsRequired().HasMaxLength(10);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.CompressedSize);
                entity.Property(e => e.IsCompressed).IsRequired();
                entity.Property(e => e.CompressionType).HasMaxLength(20);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OwnerId).IsRequired();
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.UploadedAt).IsRequired();
                entity.Property(e => e.IsDeleted).IsRequired();
                
                // Configure relationship
                entity.HasOne(f => f.Owner)
                      .WithMany()
                      .HasForeignKey(f => f.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                // Create index for performance
                entity.HasIndex(e => new { e.OwnerId, e.FileType });
                entity.HasIndex(e => e.StoredFileName).IsUnique();
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