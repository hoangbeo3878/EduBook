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
    [Route("api/tutors")]
    public class TutorsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TutorsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/tutors?subjectId=1 -> lấy ds tutor, có thể lọc theo subjectId
        [HttpGet]
        public async Task<ActionResult<List<TutorListItemDto>>> GetList([FromQuery] int? subjectId)
        {
            var query = _db.TutorProfiles
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .AsQueryable();

            if (subjectId.HasValue)
            {
                query = query.Where(t =>
                    t.TutorSubjects.Any(ts => ts.SubjectId == subjectId.Value));
            }

            var list = await query
                .OrderBy(t => t.DisplayName)
                .Select(t => new TutorListItemDto(
                    t.Id,
                    t.DisplayName,
                    t.HourlyRate,
                    t.TutorSubjects.Select(ts => ts.Subject.Name).ToList()
                ))
                .ToListAsync();

            return Ok(list);
        }

        // GET /api/tutors/{id} —> lấy chi tiết hồ sơ tutor
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TutorDetailDto>> GetById(Guid id)
        {
            var t = await _db.TutorProfiles
                .AsNoTracking()
                .Include(x => x.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (t == null)
                return NotFound(new { message = "Tutor not found." });

            var dto = new TutorDetailDto(
                t.Id,
                t.UserId,
                t.DisplayName,
                t.Bio,
                t.HourlyRate,
                t.IsActive,
                t.TutorSubjects
                    .Select(ts => new SubjectDto(ts.Subject.Id, ts.Subject.Name, ts.Subject.Description))
                    .ToList()
            );

            return Ok(dto);
        }

        // POST /api/tutors/me  —> tạo hồ sơ (chỉ Tutor)
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPost("me")]
        public async Task<ActionResult<TutorDetailDto>> CreateMyProfile(
            [FromBody] UpsertTutorProfileRequest req)
        {
            var userId = GetUserIdOrThrow();

            var exists = await _db.TutorProfiles.AnyAsync(t => t.UserId == userId);
            if (exists)
                return Conflict(new { message = "Tutor profile already exists. Use PUT to update." });

            if (req.HourlyRate < 0)
                return BadRequest(new { message = "HourlyRate must be >= 0." });

            var subjectIds = (req.SubjectIds ?? new List<int>()).Distinct().ToList();
            var validCount = await _db.Subjects.CountAsync(s => subjectIds.Contains(s.Id));
            if (subjectIds.Count != validCount)
                return BadRequest(new { message = "One or more subjectIds are invalid." });

            var profile = new TutorProfile
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                DisplayName = req.DisplayName.Trim(),
                Bio = req.Bio,
                HourlyRate = req.HourlyRate,
                IsActive = true
            };

            foreach (var sid in subjectIds)
            {
                profile.TutorSubjects.Add(new TutorSubject
                {
                    TutorProfileId = profile.Id,
                    SubjectId = sid
                });
            }

            _db.TutorProfiles.Add(profile);
            await _db.SaveChangesAsync();

            return await GetById(profile.Id);
        }

        // PUT /api/tutors/me -> cập nhật hồ sơ của chính tutor đang đăng nhập
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpPut("me")]
        public async Task<ActionResult<TutorDetailDto>> UpdateMyProfile(
            [FromBody] UpsertTutorProfileRequest req)
        {
            var userId = GetUserIdOrThrow();

            var profile = await _db.TutorProfiles
                .Include(t => t.TutorSubjects)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Create profile first (POST /api/tutors/me)." });

            if (req.HourlyRate < 0)
                return BadRequest(new { message = "HourlyRate must be >= 0." });

            var subjectIds = (req.SubjectIds ?? new List<int>()).Distinct().ToList();
            var validCount = await _db.Subjects.CountAsync(s => subjectIds.Contains(s.Id));
            if (subjectIds.Count != validCount)
                return BadRequest(new { message = "One or more subjectIds are invalid." });

            profile.DisplayName = req.DisplayName.Trim();
            profile.Bio = req.Bio;
            profile.HourlyRate = req.HourlyRate;

            _db.TutorSubjects.RemoveRange(profile.TutorSubjects);
            profile.TutorSubjects = subjectIds
                .Select(sid => new TutorSubject
                {
                    TutorProfileId = profile.Id,
                    SubjectId = sid
                })
                .ToList();

            await _db.SaveChangesAsync();
            return await GetById(profile.Id);
        }

        // GET /api/tutors/me -> lấy hồ sơ của chính tutor đang đăng nhập
        [Authorize(Roles = UserRoles.Tutor)]
        [HttpGet("me")]
        public async Task<ActionResult<TutorDetailDto>> GetMe()
        {
            var userId = GetUserIdOrThrow(); // hoặc pattern Guid? như Students

            var t = await _db.TutorProfiles
                .AsNoTracking()
                .Include(x => x.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (t == null)
            {
                return NotFound(new { message = "Profile not found. POST /api/tutors/me first." });
            }

            return Ok(new TutorDetailDto(
                t.Id,
                t.UserId,
                t.DisplayName,
                t.Bio,
                t.HourlyRate,
                t.IsActive,
                t.TutorSubjects
                    .Select(ts => new SubjectDto(
                        ts.Subject.Id, ts.Subject.Name, ts.Subject.Description))
                    .ToList()
            ));
        }

        private Guid GetUserIdOrThrow()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var id))
                throw new UnauthorizedAccessException();
            return id;
        }
    }
}