using System.ComponentModel.DataAnnotations;

namespace EduBook.Application.DTOs
{
    public record UpsertTutorProfileRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string DisplayName { get; init; } = string.Empty;

        [StringLength(2000)]
        public string? Bio { get; init; }

        [Range(0, 999999)]
        public decimal HourlyRate { get; init; }

        public List<int>? SubjectIds { get; init; }
    } 

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