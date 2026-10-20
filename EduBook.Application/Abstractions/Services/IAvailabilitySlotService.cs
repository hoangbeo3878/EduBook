using EduBook.Application.DTOs;

namespace EduBook.Application.Abstractions.Services;

public interface IAvailabilitySlotService
{
    Task<AvailabilitySlotDto> CreateAsync(
        Guid userId,
        CreateAvailabilitySlotRequest request);

    Task<IReadOnlyList<AvailabilitySlotDto>> GetMySlotsAsync(
        Guid userId);

    Task DeleteAsync(
        Guid userId,
        Guid slotId);
}