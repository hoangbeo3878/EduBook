using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _db;
        private static readonly TimeSpan CancelMinBefore = TimeSpan.FromHours(4);

        public BookingService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BookingDto> CreateAsync(Guid studentId, CreateBookingRequest request)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var slot = await _db.AvailabilitySlots
                .Include(s => s.TutorProfile)
                .FirstOrDefaultAsync(s => s.Id == request.SlotId);

            if (slot == null)
                throw new KeyNotFoundException("Slot not found.");

            if (slot.Status != SlotStatus.Open)
                throw new InvalidOperationException("Slot is not available.");

            if (slot.StartUtc <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Cannot book a slot in the past.");

            // Tutor không book slot của chính mình
            if (slot.TutorProfile.UserId == studentId)
                throw new InvalidOperationException("Cannot book your own slot.");

            var already = await _db.Bookings.AnyAsync(b => b.SlotId == slot.Id);
            if (already)
                throw new InvalidOperationException("Slot already booked.");

            slot.Status = SlotStatus.Booked;

            var booking = new Domain.Entities.Booking
            {
                Id = Guid.CreateVersion7(),
                SlotId = slot.Id,
                StudentId = studentId,
                Status = BookingStatus.Confirmed,
                Note = request.Note,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return await MapDto(booking.Id);
        }

        public async Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, bool asTutor)
        {
            IQueryable<Domain.Entities.Booking> query = _db.Bookings
                .AsNoTracking()
                .Include(b => b.Slot)
                    .ThenInclude(s => s.TutorProfile);

            if (asTutor)
            {
                query = query.Where(b => b.Slot.TutorProfile.UserId == userId);
            }
            else
            {
                query = query.Where(b => b.StudentId == userId);
            }

            var list = await query
                .OrderByDescending(b => b.Slot.StartUtc)
                .ToListAsync();

            return list.Select(b => ToDto(b)).ToList();
        }

        public async Task CancelAsync(Guid userId, Guid bookingId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var booking = await _db.Bookings
                .Include(b => b.Slot)
                    .ThenInclude(s => s.TutorProfile)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking already cancelled.");

            var isStudent = booking.StudentId == userId;
            var isTutor = booking.Slot.TutorProfile.UserId == userId;
            if (!isStudent && !isTutor)
                throw new UnauthorizedAccessException("Not your booking.");

            var remain = booking.Slot.StartUtc - DateTimeOffset.UtcNow;
            if (remain < CancelMinBefore)
                throw new InvalidOperationException(
                    $"Cancel only allowed at least {CancelMinBefore.TotalHours} hours before start.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTimeOffset.UtcNow;
            booking.Slot.Status = SlotStatus.Open; // trả slot

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        private async Task<BookingDto> MapDto(Guid id)
        {
            var b = await _db.Bookings
                .AsNoTracking()
                .Include(x => x.Slot)
                    .ThenInclude(s => s.TutorProfile)
                .FirstAsync(x => x.Id == id);

            return ToDto(b);
        }

        private static BookingDto ToDto(Domain.Entities.Booking b)
        {
            return new BookingDto(
                b.Id,
                b.SlotId,
                b.StudentId,
                b.Slot.TutorProfileId,
                b.Slot.TutorProfile.DisplayName,
                b.Slot.StartUtc,
                b.Slot.EndUtc,
                b.Status.ToString(),
                b.Note,
                b.CreatedAt
            );
        }
    }
}