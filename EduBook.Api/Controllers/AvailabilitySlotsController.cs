
using EduBook.Api.Extensions;
using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Api.Controllers;

[ApiController]
[Route("api/availability-slots")]
[Authorize(Roles = UserRoles.Tutor)]
public class AvailabilitySlotsController : ControllerBase
{
    private readonly IAvailabilitySlotService _service;

    public AvailabilitySlotsController(
        IAvailabilitySlotService service)
    {
        _service = service;
    }

    // POST /api/availability-slots
    [HttpPost]
    public async Task<ActionResult<AvailabilitySlotDto>> Create(
        [FromBody] CreateAvailabilitySlotRequest request)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await _service.CreateAsync(
                userId,
                request);

            return CreatedAtAction(
                nameof(GetMySlots),
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // GET /api/availability-slots/me
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotDto>>>
        GetMySlots()
    {
        try
        {
            var userId = User.GetUserId();

            var slots = await _service.GetMySlotsAsync(userId);

            return Ok(slots);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE /api/availability-slots/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = User.GetUserId();

            await _service.DeleteAsync(userId, id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}
