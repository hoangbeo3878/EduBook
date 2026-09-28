using EduBook.Domain.Enums;

namespace EduBook.Domain.Entities;

public class AvailabilitySlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TutorProfileId { get; set; }
    public TutorProfile TutorProfile { get; set; } = null!;

    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public SlotStatus Status { get; set; } = SlotStatus.Open;

    public Booking? Booking { get; set; }
}