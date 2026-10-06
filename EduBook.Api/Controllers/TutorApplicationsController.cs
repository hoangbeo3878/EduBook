using EduBook.Api.Extensions;
using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Api.Controllers;

[ApiController]
[Route("api/tutor-applications")]
[Authorize(Roles = UserRoles.Student)]
public class TutorApplicationsController : ControllerBase
{
    private readonly ITutorApplicationService _tutorApplications;

    public TutorApplicationsController(
        ITutorApplicationService tutorApplications)
    {
        _tutorApplications = tutorApplications;
    }

    // POST: api/tutor-applications -> Create a new tutor application
    [HttpPost]
    public async Task<ActionResult<TutorApplicationDto>> Create(
        [FromBody] CreateTutorApplicationRequest request)
    {
        try
        {
            var userId = User.GetUserId();

            var application =
                await _tutorApplications.CreateAsync(
                    userId,
                    request);

            return Ok(application);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // GET: api/tutor-applications/me -> Get all tutor applications of the current user
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<TutorApplicationDto>>>
        GetMyApplications()
    {
        var userId = User.GetUserId();

        var applications =
            await _tutorApplications.GetMyApplicationsAsync(userId);

        return Ok(applications);
    }
}