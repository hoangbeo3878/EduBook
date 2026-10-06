using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<TutorSubject> TutorSubjects => Set<TutorSubject>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<StudentSubject> StudentSubjects => Set<StudentSubject>();
    public DbSet<TutorApplication> TutorApplications => Set<TutorApplication>();
    public DbSet<TutorApplicationSubject> TutorApplicationSubjects => Set<TutorApplicationSubject>();
    public DbSet<TutorApplicationAvailability> TutorApplicationAvailabilities => Set<TutorApplicationAvailability>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tutor ↔ Subject (nhiều-nhiều)
        builder.Entity<TutorSubject>(e =>
        {
            e.HasKey(x => new { x.TutorProfileId, x.SubjectId });

            e.HasOne(x => x.TutorProfile)
                .WithMany(t => t.TutorSubjects)
                .HasForeignKey(x => x.TutorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Subject)
                .WithMany(s => s.TutorSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Subject.IsDeleted);
        });

        // User 1-1 TutorProfile
        builder.Entity<TutorProfile>(e =>
        {
            e.HasOne(x => x.User)
                .WithOne(u => u.TutorProfile)
                .HasForeignKey<TutorProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.HourlyRate).HasPrecision(18, 2);
            e.Property(x => x.DisplayName).HasMaxLength(200);
            e.Property(x => x.Bio).HasMaxLength(2000);
        });

        // Subject
        builder.Entity<Subject>(e =>
        {
            e.Property(x => x.Name)
               .HasMaxLength(200)
               .IsRequired();

            e.HasQueryFilter(x => !x.IsDeleted);
        });

        // Slot thuộc Tutor
        builder.Entity<AvailabilitySlot>(e =>
        {
            e.HasOne(x => x.TutorProfile)
                .WithMany(t => t.Slots)
                .HasForeignKey(x => x.TutorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.TutorProfileId, x.StartUtc });
        });

        builder.Entity<StudentProfile>(e =>
        {
            e.HasOne(x => x.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<StudentProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.DisplayName).HasMaxLength(200);
        });

        builder.Entity<StudentSubject>(e =>
        {
            e.HasKey(x => new { x.StudentProfileId, x.SubjectId });

            e.HasOne(x => x.StudentProfile)
                .WithMany(p => p.PreferredSubjects)
                .HasForeignKey(x => x.StudentProfileId);

            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(x => !x.Subject.IsDeleted);
        });

        // Booking 1-1 Slot + thuộc Student
        builder.Entity<Booking>(e =>
        {
            e.HasOne(x => x.Slot)
                .WithOne(s => s.Booking)
                .HasForeignKey<Booking>(x => x.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Student)
                .WithMany(u => u.BookingsAsStudent)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Một slot chỉ một booking (chống double-book ở DB)
            e.HasIndex(x => x.SlotId).IsUnique();

            e.Property(x => x.Note).HasMaxLength(500);
        });

        // TutorApplication
        builder.Entity<TutorApplication>(e =>
        {
            e.Property(x => x.Qualifications)
                .HasMaxLength(2000)
                .IsRequired();

            e.Property(x => x.Introduction)
                .HasMaxLength(2000);

            e.Property(x => x.AdminNote)
                .HasMaxLength(2000);

            e.HasOne(x => x.User)
                .WithMany(u => u.TutorApplications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("[Status] = 0");
        });

        // TutorApplication ↔ Subject (nhiều-nhiều)
        builder.Entity<TutorApplicationSubject>(e =>
        {
            e.HasKey(x => new
            {
                x.TutorApplicationId,
                x.SubjectId
            });

            e.HasOne(x => x.TutorApplication)
                .WithMany(a => a.Subjects)
                .HasForeignKey(x => x.TutorApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TutorApplication ↔ Availability (nhiều-nhiều)
        builder.Entity<TutorApplicationAvailability>(e =>
        {
            e.HasOne(x => x.TutorApplication)
                .WithMany(a => a.Availabilities)
                .HasForeignKey(x => x.TutorApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new
            {
                x.TutorApplicationId,
                x.DayOfWeek,
                x.StartTime,
                x.EndTime
            })
            .IsUnique();
        });
    }
}