using System;
using System.Collections.Generic;
using System.Text;

namespace EduBook.Application.DTOs
{
    public record UpsertStudentProfileRequest(
        string DisplayName,
        string? Bio,
        List<int> PreferredSubjectIds
    );

    public record StudentProfileDto(
        Guid Id,
        Guid UserId,
        string DisplayName,
        string? Bio,
        List<SubjectDto> PreferredSubjects
    );
}
