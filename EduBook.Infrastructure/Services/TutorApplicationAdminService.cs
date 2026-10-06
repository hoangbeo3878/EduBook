using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Infrastructure.Services;

public class TutorApplicationAdminService
    : ITutorApplicationAdminService
{
    private readonly AppDbContext _db;

    public TutorApplicationAdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminTutorApplicationListItemDto>> GetAsync(
        AdminTutorApplicationQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => query.PageSize
        };

        IQueryable<TutorApplication> applications =
            _db.TutorApplications
                .AsNoTracking()
                .Include(x => x.User);

        if (query.Status.HasValue)
        {
            applications = applications.Where(
                x => x.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            applications = applications.Where(x =>
                (x.User.FullName != null &&
                 x.User.FullName.Contains(search)) ||
                (x.User.Email != null &&
                 x.User.Email.Contains(search)));
        }

        var totalCount = await applications.CountAsync();

        var items = await applications
            .OrderByDescending(x => x.SubmittedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminTutorApplicationListItemDto(
                x.Id,
                x.UserId,
                x.User.FullName ?? string.Empty,
                x.User.Email ?? string.Empty,
                x.Status,
                x.SubmittedAt,
                x.ReviewedAt
            ))
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResult<AdminTutorApplicationListItemDto>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<AdminTutorApplicationDetailDto> GetByIdAsync(
        Guid id)
    {
        var application = await _db.TutorApplications
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Subjects)
                .ThenInclude(x => x.Subject)
            .Include(x => x.Availabilities)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (application == null)
        {
            throw new KeyNotFoundException(
                "Tutor application not found.");
        }

        var subjects = application.Subjects
            .OrderBy(x => x.Subject.Name)
            .Select(x => new SubjectDto(
                x.Subject.Id,
                x.Subject.Name,
                x.Subject.Description))
            .ToList();

        var availabilities = application.Availabilities
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(x => new TutorApplicationAvailabilityDto(
                x.DayOfWeek,
                x.StartTime,
                x.EndTime))
            .ToList();

        return new AdminTutorApplicationDetailDto(
            application.Id,
            application.UserId,
            application.User.FullName ?? string.Empty,
            application.User.Email ?? string.Empty,
            application.Status,
            application.Qualifications,
            application.Introduction,
            subjects,
            availabilities,
            application.AdminNote,
            application.ReviewedByUserId,
            application.SubmittedAt,
            application.ReviewedAt);
    }
}