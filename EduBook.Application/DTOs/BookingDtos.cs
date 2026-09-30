namespace EduBook.Application.DTOs
{
    public record CreateBookingRequest(Guid SlotId, string? Note);

    public record BookingDto(
        Guid Id,
        Guid SlotId,
        Guid StudentId,
        Guid TutorProfileId,
        string TutorDisplayName,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        string Status,
        string? Note,
        DateTimeOffset CreatedAt
    );
}