namespace EduBook.Application.DTOs
{
    public record CreateSlotRequest(
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc
    );

    public record SlotDto(
        Guid Id,
        Guid TutorProfileId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        string Status
    );

    public record CreateSlotsBulkRequest(List<CreateSlotRequest> Slots);
}