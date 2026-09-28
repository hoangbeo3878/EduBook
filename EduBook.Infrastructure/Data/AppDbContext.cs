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

        builder.Entity<Subject>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
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
    }
}