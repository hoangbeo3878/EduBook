using EduBook.Domain.Enums;

namespace EduBook.Domain.Entities;

public class TutorApplication
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Qualifications { get; set; } = string.Empty;

    public string? Introduction { get; set; }

    public TutorApplicationStatus Status { get; set; }
        = TutorApplicationStatus.Pending;

    public string? AdminNote { get; set; }

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReviewedAt { get; set; }

    public ICollection<TutorApplicationSubject> Subjects { get; set; }
        = new List<TutorApplicationSubject>();

    public ICollection<TutorApplicationAvailability> Availabilities { get; set; }
        = new List<TutorApplicationAvailability>();
}