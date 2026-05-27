using Microsoft.EntityFrameworkCore;
using backend.db.Models;

namespace backend.db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Models.Task> Tasks => Set<Models.Task>();
    public DbSet<TaskVolunteer> TaskVolunteers => Set<TaskVolunteer>();
    public DbSet<VolunteerHour> VolunteerHours => Set<VolunteerHour>();
    public DbSet<TaskImage> TaskImages => Set<TaskImage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<OrganizationReview> OrganizationReviews => Set<OrganizationReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");

            e.HasIndex(u => u.Email).IsUnique();

            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(20);
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("student");
            e.Property(u => u.UniversityId).HasMaxLength(50);
            e.Property(u => u.TakingVolunteeringCourse).HasDefaultValue(false);
            e.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            e.Property(u => u.IsBlocked).HasDefaultValue(false);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Seed default admin "Adam"
            e.HasData(new User
            {
                Id = 1,
                FullName = "Adam",
                Email = "adam@fursa.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Adam123"),
                Role = "admin",
                TakingVolunteeringCourse = false,
                IsBlocked = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        // ── Organization ────────────────────────────────────────
        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("organizations");

            e.HasIndex(o => o.Email).IsUnique();

            e.Property(o => o.Name).HasMaxLength(150).IsRequired();
            e.Property(o => o.Email).HasMaxLength(150).IsRequired();
            e.Property(o => o.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(o => o.PhoneNumber).HasMaxLength(20);
            e.Property(o => o.LogoUrl).HasMaxLength(500);
            e.Property(o => o.BannerUrl).HasMaxLength(500);
            e.Property(o => o.Description).HasMaxLength(1000);
            e.Property(o => o.IsApproved).HasDefaultValue(false);
            e.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ── Task ────────────────────────────────────────────────
        modelBuilder.Entity<Models.Task>(e =>
        {
            e.ToTable("tasks");

            e.Property(t => t.Title).HasMaxLength(200).IsRequired();
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("open");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(t => t.Organization)
             .WithMany(o => o.Tasks)
             .HasForeignKey(t => t.OrganizationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TaskVolunteer ───────────────────────────────────────
        modelBuilder.Entity<TaskVolunteer>(e =>
        {
            e.ToTable("task_volunteers");

            e.Property(a => a.Status).HasMaxLength(20).HasDefaultValue("pending");
            e.Property(a => a.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(a => a.Task)
             .WithMany(t => t.TaskVolunteers)
             .HasForeignKey(a => a.TaskId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.User)
             .WithMany(u => u.TaskVolunteers)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(a => new { a.TaskId, a.UserId }).IsUnique();
        });

        // ── VolunteerHour ───────────────────────────────────────
        modelBuilder.Entity<VolunteerHour>(e =>
        {
            e.ToTable("volunteer_hours");

            e.Property(w => w.HoursWorked).HasPrecision(5, 2);
            e.Property(w => w.Notes).HasMaxLength(500);
            e.Property(w => w.RecordedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(w => w.Task)
             .WithMany(t => t.VolunteerHours)
             .HasForeignKey(w => w.TaskId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(w => w.User)
             .WithMany(u => u.VolunteerHours)
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TaskImage ───────────────────────────────────────────
        modelBuilder.Entity<TaskImage>(e =>
        {
            e.ToTable("task_images");
            
            e.Property(i => i.ImageUrl).HasMaxLength(500).IsRequired();
            e.Property(i => i.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(i => i.Task)
             .WithMany(t => t.TaskImages)
             .HasForeignKey(i => i.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Tag ─────────────────────────────────────────────────
        modelBuilder.Entity<Tag>(e =>
        {
            e.ToTable("tags");

            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.Name).HasMaxLength(50).IsRequired();
        });

        // ── TaskTag ─────────────────────────────────────────────
        modelBuilder.Entity<TaskTag>(e =>
        {
            e.ToTable("task_tags");

            e.HasKey(tt => new { tt.TaskId, tt.TagId });

            e.HasOne(tt => tt.Task)
             .WithMany(t => t.TaskTags)
             .HasForeignKey(tt => tt.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
             
            e.HasOne(tt => tt.Tag)
             .WithMany(t => t.TaskTags)
             .HasForeignKey(tt => tt.TagId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── OrganizationReview ──────────────────────────────────────
        modelBuilder.Entity<OrganizationReview>(e =>
        {
            e.ToTable("organization_reviews");

            e.Property(r => r.Rating).IsRequired();
            e.Property(r => r.Comment).HasMaxLength(1000);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(r => r.Organization)
             .WithMany(o => o.Reviews)
             .HasForeignKey(r => r.OrganizationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.User)
             .WithMany(u => u.OrganizationReviews)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => new { r.OrganizationId, r.UserId }).IsUnique();
        });
    }
}
