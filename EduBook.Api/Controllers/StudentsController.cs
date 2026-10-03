using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduBook.Api.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize(Roles = UserRoles.Student)]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/students/me -> Lấy thông tin profile của chính user đang đăng nhập
        [HttpGet("me")]
        public async Task<ActionResult<StudentProfileDto>> GetMe()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var p = await LoadProfile(userId.Value);
            if (p == null)
                return NotFound(new { message = "Profile not created yet. PUT /api/students/me first." });

            return Ok(ToDto(p));
        }

        // PUT /api/students/me -> Tạo hoặc cập nhật profile của chính user đang đăng nhập
        [HttpPut("me")]
        public async Task<ActionResult<StudentProfileDto>> UpsertMe(
            [FromBody] UpsertStudentProfileRequest req)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var ids = (req.PreferredSubjectIds ?? new List<int>()).Distinct().ToList();
            var valid = await _db.Subjects.CountAsync(s => ids.Contains(s.Id));
            if (ids.Count != valid)
                return BadRequest(new { message = "Invalid subject id." });

            var profile = await _db.StudentProfiles
                .Include(x => x.PreferredSubjects)
                .FirstOrDefaultAsync(x => x.UserId == userId.Value);

            if (profile == null)
            {
                profile = new StudentProfile
                {
                    Id = Guid.CreateVersion7(),
                    UserId = userId.Value,
                    DisplayName = req.DisplayName.Trim(),
                    Bio = req.Bio
                };
                foreach (var sid in ids)
                    profile.PreferredSubjects.Add(new StudentSubject
                    {
                        StudentProfileId = profile.Id,
                        SubjectId = sid
                    });
                _db.StudentProfiles.Add(profile);
            }
            else
            {
                profile.DisplayName = req.DisplayName.Trim();
                profile.Bio = req.Bio;
                _db.StudentSubjects.RemoveRange(profile.PreferredSubjects);
                profile.PreferredSubjects = ids.Select(sid => new StudentSubject
                {
                    StudentProfileId = profile.Id,
                    SubjectId = sid
                }).ToList();
            }

            await _db.SaveChangesAsync();
            var loaded = await LoadProfile(userId.Value);
            return Ok(ToDto(loaded!));
        }

        private async Task<StudentProfile?> LoadProfile(Guid userId) =>
            await _db.StudentProfiles.AsNoTracking()
                .Include(p => p.PreferredSubjects).ThenInclude(ps => ps.Subject)
                .FirstOrDefaultAsync(p => p.UserId == userId);

        private static StudentProfileDto ToDto(StudentProfile p) => new(
            p.Id, p.UserId, p.DisplayName, p.Bio,
            p.PreferredSubjects.Select(x =>
                new SubjectDto(x.Subject.Id, x.Subject.Name, x.Subject.Description)).ToList());

        private Guid? GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
