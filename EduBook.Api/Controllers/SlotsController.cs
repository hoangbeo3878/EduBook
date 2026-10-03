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
        // -> Lấy ds các slot của tutor trong khoảng thời gian từ `from` đến `to`. Nếu không có `from`, mặc định là hiện tại. Nếu không có `to`, mặc định là 14 ngày sau `from`.  
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

        // POST /api/tutors/me/slots -> Tạo slot mới do Tutor hiện tại tạo ra
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

        // POST /api/tutors/me/slots/bulk -> Tạo nhiều slot cùng lúc do Tutor hiện tại tạo ra
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPost("tutors/me/slots/bulk")]
        public async Task<ActionResult<List<SlotDto>>> CreateBulk([FromBody] CreateSlotsBulkRequest req)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (req.Slots == null || req.Slots.Count == 0)
                return BadRequest(new { message = "Slots list is empty." });

            var profile = await _db.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == userId.Value && t.IsActive);
            if (profile == null)
                return BadRequest(new { message = "Create tutor profile first." });

            var ordered = req.Slots.OrderBy(s => s.StartUtc).ToList();
            var created = new List<AvailabilitySlot>();

            // 1) Validate từng phần tử + overlap trong chính list gửi lên
            for (int i = 0; i < ordered.Count; i++)
            {
                var item = ordered[i];
                if (item.EndUtc <= item.StartUtc)
                    return BadRequest(new { message = $"Invalid range at index {i}." });
                if (item.StartUtc <= DateTimeOffset.UtcNow)
                    return BadRequest(new { message = $"Start must be future at index {i}." });

                for (int j = i + 1; j < ordered.Count; j++)
                {
                    var other = ordered[j];
                    if (item.StartUtc < other.EndUtc && other.StartUtc < item.EndUtc)
                        return Conflict(new { message = "Slots in request overlap each other." });
                }
            }

            // 2) Overlap với DB
            foreach (var item in ordered)
            {
                var overlapDb = await _db.AvailabilitySlots.AnyAsync(s =>
                    s.TutorProfileId == profile.Id
                    && s.Status != SlotStatus.Blocked
                    && s.StartUtc < item.EndUtc
                    && item.StartUtc < s.EndUtc);

                if (overlapDb)
                    return Conflict(new { message = $"Overlaps existing slot: {item.StartUtc}" });

                created.Add(new AvailabilitySlot
                {
                    Id = Guid.CreateVersion7(),
                    TutorProfileId = profile.Id,
                    StartUtc = item.StartUtc,
                    EndUtc = item.EndUtc,
                    Status = SlotStatus.Open
                });
            }

            _db.AvailabilitySlots.AddRange(created);
            await _db.SaveChangesAsync();

            return Ok(created.Select(s => new SlotDto(
                s.Id, s.TutorProfileId, s.StartUtc, s.EndUtc, s.Status.ToString())).ToList());
        }

        // DELETE /api/tutors/me/slots/{slotId} -> Xóa slot do Tutor hiện tại tạo ra (nếu chưa được book)
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

        // GET /api/tutors/me/slots -> Lấy ds các slot của Tutor hiện tại (không lọc Status)
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpGet("tutors/me/slots")]
        public async Task<ActionResult<List<SlotDto>>> GetMySlots()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _db.TutorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);

            if (profile == null)
            {
                return NotFound(new { message = "Tutor profile required." });
            }

            // Không lọc Status → tutor thấy tất cả
            var slots = await _db.AvailabilitySlots
                .AsNoTracking()
                .Where(s => s.TutorProfileId == profile.Id)
                .OrderBy(s => s.StartUtc)
                .Select(s => new SlotDto(
                    s.Id, s.TutorProfileId, s.StartUtc, s.EndUtc, s.Status.ToString()))
                .ToListAsync();

            return Ok(slots);
        }

        // POST /api/tutors/me/slots/{slotId}/block -> Block slot do Tutor hiện tại tạo ra (nếu chưa được book)
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPost("tutors/me/slots/{slotId:guid}/block")]
        public async Task<ActionResult<SlotDto>> Block(Guid slotId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var slot = await _db.AvailabilitySlots
                .Include(s => s.TutorProfile)
                .FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null) return NotFound();

            // Chỉ chủ slot
            if (slot.TutorProfile.UserId != userId.Value)
                return Forbid();

            if (slot.Status == SlotStatus.Booked)
            {
                return Conflict(new { message = "Cannot block a booked slot. Cancel booking first." });
            }

            slot.Status = SlotStatus.Blocked;
            await _db.SaveChangesAsync();

            return Ok(new SlotDto(
                slot.Id, slot.TutorProfileId, slot.StartUtc, slot.EndUtc, slot.Status.ToString()));
        }

        // POST /api/tutors/me/slots/{slotId}/unblock -> Unblock slot do Tutor hiện tại đang block
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPost("tutors/me/slots/{slotId:guid}/unblock")]
        public async Task<ActionResult<SlotDto>> Unblock(Guid slotId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var slot = await _db.AvailabilitySlots
                .Include(s => s.TutorProfile)
                .FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null) return NotFound();
            // Chỉ chủ slot
            if (slot.TutorProfile.UserId != userId.Value)
                return Forbid();
            if (slot.Status != SlotStatus.Blocked)
            {
                return Conflict(new { message = "Slot is not blocked." });
            }
            slot.Status = SlotStatus.Open;
            await _db.SaveChangesAsync();
            return Ok(new SlotDto(
                slot.Id, slot.TutorProfileId, slot.StartUtc, slot.EndUtc, slot.Status.ToString()));

        }
    }
        
}