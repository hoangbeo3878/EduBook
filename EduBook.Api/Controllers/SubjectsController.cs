using EduBook.Application.DTOs;
using EduBook.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Api.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SubjectsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/subjects
        [HttpGet]
        public async Task<ActionResult<List<SubjectDto>>> GetAll()
        {
            var list = await _db.Subjects
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SubjectDto(s.Id, s.Name, s.Description))
                .ToListAsync();

            return Ok(list);
        }
    }
}