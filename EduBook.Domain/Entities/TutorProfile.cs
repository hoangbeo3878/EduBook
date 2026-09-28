namespace EduBook.Domain.Entities;

public class TutorProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
    public ICollection<AvailabilitySlot> Slots { get; set; } = new List<AvailabilitySlot>();
}