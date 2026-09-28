using EduBook.Domain.Enums;

namespace EduBook.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SlotId { get; set; }
    public AvailabilitySlot Slot { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CancelledAt { get; set; }
}