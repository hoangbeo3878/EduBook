using EduBook.Application.DTOs;

namespace EduBook.Application.Abstractions.Services;

public interface ITutorApplicationService
{
    Task<TutorApplicationDto> CreateAsync(
        Guid userId,
        CreateTutorApplicationRequest request);

    Task<IReadOnlyList<TutorApplicationDto>> GetMyApplicationsAsync(
        Guid userId);
}