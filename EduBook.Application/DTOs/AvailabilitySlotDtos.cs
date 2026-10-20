using System.ComponentModel.DataAnnotations;
using EduBook.Domain.Enums;

namespace EduBook.Application.DTOs;

public record CreateAvailabilitySlotRequest
{
    [Required]
    public DateTimeOffset StartUtc { get; init; }

    [Required]
    public DateTimeOffset EndUtc { get; init; }
}

public record AvailabilitySlotDto(
    Guid Id,
    Guid TutorProfileId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    SlotStatus Status
);