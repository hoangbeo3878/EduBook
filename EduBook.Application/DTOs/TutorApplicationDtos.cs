using EduBook.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduBook.Application.DTOs;

public record CreateTutorApplicationRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Qualifications { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Introduction { get; init; }

    public List<int> SubjectIds { get; init; } = new();

    public List<TutorApplicationAvailabilityRequest> Availabilities { get; init; }
        = new();
}

public record TutorApplicationAvailabilityRequest
{
    [Range(0, 6)]
    public DayOfWeek DayOfWeek { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}

public record TutorApplicationDto(
    Guid Id,
    TutorApplicationStatus Status,
    string Qualifications,
    string? Introduction,
    IReadOnlyList<SubjectDto> Subjects,
    IReadOnlyList<TutorApplicationAvailabilityDto> Availabilities,
    string? AdminNote,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ReviewedAt
);

public record TutorApplicationAvailabilityDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);