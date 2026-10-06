using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Api.Controllers;

[ApiController]
[Route("api/admin/tutor-applications")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminTutorApplicationsController : ControllerBase
{
    private readonly ITutorApplicationAdminService _service;

    public AdminTutorApplicationsController(
        ITutorApplicationAdminService service)
    {
        _service = service;
    }

    // GET: api/admin/tutor-applications 
    // /api/admin/tutor-applications?status=Pending 
    // GET /api/admin/tutor-applications?search=nguyen 
    // GET /api/admin/tutor-applications?page=2&pageSize=10
    // -> Get a paginated list of tutor applications with optional filtering
    [HttpGet]
    public async Task<
        ActionResult<PagedResult<AdminTutorApplicationListItemDto>>>
        Get(
            [FromQuery] TutorApplicationStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
    {
        var query = new AdminTutorApplicationQuery(
            status,
            search,
            page,
            pageSize);

        var result = await _service.GetAsync(query);

        return Ok(result);
    }

    // GET: api/admin/tutor-applications/{id} -> Get a specific tutor application by ID
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminTutorApplicationDetailDto>>
        GetById(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
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