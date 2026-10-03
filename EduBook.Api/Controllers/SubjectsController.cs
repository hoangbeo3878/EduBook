using EduBook.Application.DTOs;
using EduBook.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;



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

        // GET /api/subjects -> Get tất cả Subjects hiện đang có
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

        // POST /api/subjects -> Tạo mới một Subject, chỉ Admin mới có quyền tạo
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<SubjectDto>> Create(
            [FromBody] CreateSubjectRequest req)
        {
            var name = req.Name.Trim();

            var existingSubject = await _db.Subjects
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == name);

            if (existingSubject != null)
            {
                if (existingSubject.IsDeleted)
                {
                    return Conflict(new
                    {
                        message = "Subject already exists but is deleted. Restore it instead."
                    });
                }

                return Conflict(new
                {
                    message = "Subject name already exists."
                });
            }

            var subject = new Subject
            {
                Name = name,
                Description = req.Description?.Trim()
            };

            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync();

            return Ok(new SubjectDto(
                subject.Id,
                subject.Name,
                subject.Description
            ));
        }

        // PUT /api/subjects/{id} -> Cập nhật thông tin Subject, chỉ Admin mới có quyền cập nhật
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<ActionResult<SubjectDto>> Update(int id,
            [FromBody] UpdateSubjectRequest req)
        {
            var subject = await _db.Subjects
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
            {
                return NotFound(new
                {
                    message = "Subject not found."
                });
            }

            var name = req.Name.Trim();

            var existingSubject = await _db.Subjects
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id != id &&
                    s.Name == name);

            if (existingSubject != null)
            {
                return Conflict(new
                {
                    message = "Subject name already exists."
                });
            }

            subject.Name = name;
            subject.Description = req.Description?.Trim();

            await _db.SaveChangesAsync();

            return Ok(new SubjectDto(
                subject.Id,
                subject.Name,
                subject.Description
            ));
        }

        // DELETE /api/subjects/{id} -> Soft delete Subject, chỉ Admin mới có quyền xóa
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _db.Subjects
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subject == null)
            {
                return NotFound(new
                {
                    message = "Subject not found."
                });
            }

            subject.IsDeleted = true;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /api/subjects/{id}/restore -> Restore Subject, chỉ Admin mới có quyền restore
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var subject = await _db.Subjects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subject == null)
            {
                return NotFound(new
                {
                    message = "Subject not found."
                });
            }

            if (!subject.IsDeleted)
            {
                return BadRequest(new
                {
                    message = "Subject is already active."
                });
            }

            subject.IsDeleted = false;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}