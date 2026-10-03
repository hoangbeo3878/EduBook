using System.ComponentModel.DataAnnotations;

namespace EduBook.Application.DTOs;

public record SubjectDto(
    int Id,
    string Name,
    string? Description
);

public record CreateSubjectRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }
}

public record UpdateSubjectRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }
}