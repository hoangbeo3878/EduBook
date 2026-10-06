using EduBook.Application.DTOs;

namespace EduBook.Application.Abstractions.Services;

public interface ITutorApplicationAdminService
{
    Task<PagedResult<AdminTutorApplicationListItemDto>> GetAsync(
        AdminTutorApplicationQuery query);

    Task<AdminTutorApplicationDetailDto> GetByIdAsync(
        Guid id);
}