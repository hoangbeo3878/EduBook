namespace EduBook.Application.DTOs
{
    public record UpsertTutorProfileRequest(
        string DisplayName,
        string? Bio,
        decimal HourlyRate,
        List<int> SubjectIds
    );

    public record TutorListItemDto(
        Guid TutorProfileId,
        string DisplayName,
        decimal HourlyRate,
        List<string> Subjects
    );

    public record TutorDetailDto(
        Guid TutorProfileId,
        Guid UserId,
        string DisplayName,
        string? Bio,
        decimal HourlyRate,
        bool IsActive,
        List<SubjectDto> Subjects
    );
}