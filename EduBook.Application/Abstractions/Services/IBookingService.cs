using EduBook.Application.DTOs;

namespace EduBook.Application.Abstractions.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(Guid studentId, CreateBookingRequest request);
        Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, bool asTutor);
        Task CancelAsync(Guid userId, Guid bookingId);
    }
}