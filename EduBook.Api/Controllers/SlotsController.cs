using System.Security.Claims;
using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class SlotsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SlotsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/tutors/{tutorId}/slots?from=&to=
        [HttpGet("tutors/{tutorId:guid}/slots")]
        public async Task<ActionResult<List<SlotDto>>> GetTutorSlots(
            Guid tutorId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            var start = from ?? DateTimeOffset.UtcNow;
            var end = to ?? start.AddDays(14);

            var slots = await _db.AvailabilitySlots
                .AsNoTracking()
                .Where(s =>
                    s.TutorProfileId == tutorId
                    && s.Status == SlotStatus.Open
                    && s.StartUtc >= start
                    && s.StartUtc < end)
                .OrderBy(s => s.StartUtc)
                .Select(s => new SlotDto(
                    s.Id,
                    s.TutorProfileId,
                    s.StartUtc,
                    s.EndUtc,
                    s.Status.ToString()
                ))
                .ToListAsync();

            return Ok(slots);
        }

        // POST /api/tutors/me/slots
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPost("tutors/me/slots")]
        public async Task<ActionResult<SlotDto>> CreateMySlot([FromBody] CreateSlotRequest req)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (req.EndUtc <= req.StartUtc)
                return BadRequest(new { message = "EndUtc must be after StartUtc." });

            if (req.StartUtc <= DateTimeOffset.UtcNow)
                return BadRequest(new { message = "StartUtc must be in the future." });

            var profile = await _db.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == userId.Value && t.IsActive);

            if (profile == null)
                return BadRequest(new { message = "Create tutor profile first." });

            // Tránh chồng giờ cùng tutor (đơn giản)
            var overlap = await _db.AvailabilitySlots.AnyAsync(s =>
                s.TutorProfileId == profile.Id
                && s.Status != SlotStatus.Blocked
                && s.StartUtc < req.EndUtc
                && req.StartUtc < s.EndUtc);

            if (overlap)
                return Conflict(new { message = "Slot overlaps an existing slot." });

            var slot = new AvailabilitySlot
            {
                Id = Guid.CreateVersion7(),
                TutorProfileId = profile.Id,
                StartUtc = req.StartUtc,
                EndUtc = req.EndUtc,
                Status = SlotStatus.Open
            };

            _db.AvailabilitySlots.Add(slot);
            await _db.SaveChangesAsync();

            return Ok(new SlotDto(
                slot.Id, slot.TutorProfileId, slot.StartUtc, slot.EndUtc, slot.Status.ToString()));
        }

        // DELETE /api/tutors/me/slots/{slotId}
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpDelete("tutors/me/slots/{slotId:guid}")]
        public async Task<IActionResult> DeleteMySlot(Guid slotId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var slot = await _db.AvailabilitySlots
                .Include(s => s.TutorProfile)
                .FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null)
                return NotFound();

            if (slot.TutorProfile.UserId != userId.Value)
                return Forbid();

            if (slot.Status == SlotStatus.Booked)
                return Conflict(new { message = "Cannot delete a booked slot." });

            _db.AvailabilitySlots.Remove(slot);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}