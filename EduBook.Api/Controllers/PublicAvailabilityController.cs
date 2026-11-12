
using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Api.Controllers;

[ApiController]
[Route("api/tutors/{tutorProfileId:guid}/availability")]
[AllowAnonymous]
public class PublicAvailabilityController : ControllerBase
{
    private readonly IAvailabilitySlotService _service;

    public PublicAvailabilityController(
        IAvailabilitySlotService service)
    {
        _service = service;
    }

    // GET /api/tutors/{tutorProfileId}/availability
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AvailabilitySlotDto>>>
        GetAvailability(Guid tutorProfileId)
    {
        try
        {
            var slots = await _service.GetTutorAvailabilityAsync(
                tutorProfileId);

            return Ok(slots);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}
