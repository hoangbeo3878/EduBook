using Microsoft.AspNetCore.Identity;

namespace EduBook.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public TutorProfile? TutorProfile { get; set; }
    public ICollection<Booking> BookingsAsStudent { get; set; } = new List<Booking>();

    public StudentProfile? StudentProfile { get; set; }
}