using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EduBook.Application.DTOs
{
    public record UpsertStudentProfileRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string DisplayName { get; init; } = string.Empty;

        [StringLength(2000)]
        public string? Bio { get; init; }

        public List<int>? PreferredSubjectIds { get; init; }
    }

    public record StudentProfileDto(
        Guid Id,
        Guid UserId,
        string DisplayName,
        string? Bio,
        List<SubjectDto> PreferredSubjects
    );
}
