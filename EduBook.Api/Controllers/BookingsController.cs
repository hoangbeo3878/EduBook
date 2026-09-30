using System.Security.Claims;
using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookings;

        public BookingsController(IBookingService bookings)
        {
            _bookings = bookings;
        }

        // POST /api/bookings
        [Authorize(Roles = UserRoles.Student)]
        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request)
        {
            try
            {
                var studentId = GetUserId();
                if (studentId == null) return Unauthorized();

                var dto = await _bookings.CreateAsync(studentId.Value, request);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET /api/bookings/me
        [HttpGet("me")]
        public async Task<ActionResult<IReadOnlyList<BookingDto>>> MyBookings()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var asTutor = User.IsInRole(UserRoles.Tutor);
            // Nếu vừa là tutor vừa muốn xem với tư cách student: có thể thêm query ?asTutor=true
            var list = await _bookings.GetMyBookingsAsync(userId.Value, asTutor);
            return Ok(list);
        }

        // POST /api/bookings/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                await _bookings.CancelAsync(userId.Value, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid? GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}