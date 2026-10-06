using EduBook.Application.DTOs;

namespace EduBook.Application.Abstractions.Services;

public interface ITutorApplicationAdminService
{
    Task<PagedResult<AdminTutorApplicationListItemDto>> GetAsync(
        AdminTutorApplicationQuery query);

    Task<AdminTutorApplicationDetailDto> GetByIdAsync(
        Guid id);

    Task<AdminTutorApplicationDetailDto> ApproveAsync(
        Guid applicationId,
        Guid adminUserId);

    Task<AdminTutorApplicationDetailDto> DenyAsync(
        Guid applicationId,
        Guid adminUserId,
        string adminNote);
}