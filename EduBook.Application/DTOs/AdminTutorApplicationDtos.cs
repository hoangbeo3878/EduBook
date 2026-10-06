using EduBook.Domain.Enums;

namespace EduBook.Application.DTOs;

public record AdminTutorApplicationListItemDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    TutorApplicationStatus Status,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ReviewedAt
);

public record AdminTutorApplicationDetailDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    TutorApplicationStatus Status,
    string Qualifications,
    string? Introduction,
    IReadOnlyList<SubjectDto> Subjects,
    IReadOnlyList<TutorApplicationAvailabilityDto> Availabilities,
    string? AdminNote,
    Guid? ReviewedByUserId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ReviewedAt
);

public record AdminTutorApplicationQuery(
    TutorApplicationStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);