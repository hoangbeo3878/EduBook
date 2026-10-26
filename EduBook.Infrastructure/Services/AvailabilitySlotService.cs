
using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Infrastructure.Services;

public class AvailabilitySlotService : IAvailabilitySlotService
{
    private readonly AppDbContext _db;

    public AvailabilitySlotService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AvailabilitySlotDto> CreateAsync(
        Guid userId,
        CreateAvailabilitySlotRequest request)
    {
        var startUtc = request.StartUtc.ToUniversalTime();
        var endUtc = request.EndUtc.ToUniversalTime();

        if (startUtc <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException(
                "Availability must start in the future.");
        }

        if (endUtc <= startUtc)
        {
            throw new InvalidOperationException(
                "End time must be later than start time.");
        }

        var tutorProfile = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (tutorProfile == null)
        {
            throw new InvalidOperationException(
                "Tutor profile not found. Create your tutor profile first.");
        }

        // Check whether this slot overlaps any existing slot
        // belonging to the same tutor.
        await using var transaction =
    await _db.Database.BeginTransactionAsync(
        System.Data.IsolationLevel.Serializable);

        var hasOverlap = await _db.AvailabilitySlots
            .AnyAsync(x =>
                x.TutorProfileId == tutorProfile.Id &&
                x.StartUtc < endUtc &&
                x.EndUtc > startUtc);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "This availability overlaps with an existing slot.");
        }

        var slot = new AvailabilitySlot
        {
            Id = Guid.CreateVersion7(),
            TutorProfileId = tutorProfile.Id,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Status = SlotStatus.Open
        };

        _db.AvailabilitySlots.Add(slot);

        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return ToDto(slot);
    }

    public async Task<IReadOnlyList<AvailabilitySlotDto>> GetMySlotsAsync(
        Guid userId)
    {
        var tutorProfile = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (tutorProfile == null)
        {
            throw new InvalidOperationException(
                "Tutor profile not found. Create your tutor profile first.");
        }

        var slots = await _db.AvailabilitySlots
            .AsNoTracking()
            .Where(x => x.TutorProfileId == tutorProfile.Id)
            .OrderBy(x => x.StartUtc)
            .Select(x => new AvailabilitySlotDto(
                x.Id,
                x.TutorProfileId,
                x.StartUtc,
                x.EndUtc,
                x.Status))
            .ToListAsync();

        return slots;
    }

    public async Task DeleteAsync(Guid userId, Guid slotId)
    {
        var slot = await _db.AvailabilitySlots
            .Include(x => x.TutorProfile)
            .FirstOrDefaultAsync(x => x.Id == slotId);

        if (slot == null)
        {
            throw new KeyNotFoundException(
                "Availability slot not found.");
        }

        if (slot.TutorProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to delete this availability slot.");
        }

        if (slot.Status != SlotStatus.Open)
        {
            throw new InvalidOperationException(
                "Only open availability slots can be deleted.");
        }

        _db.AvailabilitySlots.Remove(slot);

        await _db.SaveChangesAsync();
    }

    private static AvailabilitySlotDto ToDto(
        AvailabilitySlot slot)
    {
        return new AvailabilitySlotDto(
            slot.Id,
            slot.TutorProfileId,
            slot.StartUtc,
            slot.EndUtc,
            slot.Status);
    }
}
