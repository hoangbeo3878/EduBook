using EduBook.Application.Abstractions.Services;
using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBook.Infrastructure.Services;

public class TutorApplicationService : ITutorApplicationService
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public TutorApplicationService(
        AppDbContext db,
        UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<TutorApplicationDto> CreateAsync(
        Guid userId,
        CreateTutorApplicationRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (await _userManager.IsInRoleAsync(user, UserRoles.Tutor))
            throw new InvalidOperationException(
                "User is already a Tutor.");

        var hasPending = await _db.TutorApplications
            .AnyAsync(x =>
                x.UserId == userId &&
                x.Status == TutorApplicationStatus.Pending);

        if (hasPending)
            throw new InvalidOperationException(
                "You already have a pending tutor application.");

        var subjectIds = request.SubjectIds
            .Distinct()
            .ToList();

        if (subjectIds.Count == 0)
            throw new InvalidOperationException(
                "At least one subject is required.");

        var subjects = await _db.Subjects
            .Where(x => subjectIds.Contains(x.Id))
            .ToListAsync();

        if (subjects.Count != subjectIds.Count)
            throw new InvalidOperationException(
                "One or more subjects are invalid.");

        if (request.Availabilities.Count == 0)
            throw new InvalidOperationException(
                "At least one availability is required.");

        ValidateAvailabilities(request.Availabilities);

        var application = new TutorApplication
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Qualifications = request.Qualifications.Trim(),
            Introduction = string.IsNullOrWhiteSpace(request.Introduction)
                ? null
                : request.Introduction.Trim(),
            Status = TutorApplicationStatus.Pending,
            SubmittedAt = DateTimeOffset.UtcNow
        };

        foreach (var subjectId in subjectIds)
        {
            application.Subjects.Add(
                new TutorApplicationSubject
                {
                    TutorApplicationId = application.Id,
                    SubjectId = subjectId
                });
        }

        foreach (var availability in request.Availabilities)
        {
            application.Availabilities.Add(
                new TutorApplicationAvailability
                {
                    TutorApplicationId = application.Id,
                    DayOfWeek = availability.DayOfWeek,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime
                });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        _db.TutorApplications.Add(application);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetByIdAsync(application.Id);
    }

    public async Task<IReadOnlyList<TutorApplicationDto>>
        GetMyApplicationsAsync(Guid userId)
    {
        var applications = await _db.TutorApplications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Subjects)
                .ThenInclude(x => x.Subject)
            .Include(x => x.Availabilities)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();

        return applications
            .Select(ToDto)
            .ToList();
    }

    private async Task<TutorApplicationDto> GetByIdAsync(Guid id)
    {
        var application = await _db.TutorApplications
            .AsNoTracking()
            .Include(x => x.Subjects)
                .ThenInclude(x => x.Subject)
            .Include(x => x.Availabilities)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (application == null)
            throw new KeyNotFoundException(
                "Tutor application not found.");

        return ToDto(application);
    }

    private static void ValidateAvailabilities(
        IEnumerable<TutorApplicationAvailabilityRequest> availabilities)
    {
        var items = availabilities.ToList();

        foreach (var item in items)
        {
            if (item.StartTime >= item.EndTime)
            {
                throw new InvalidOperationException(
                    "Availability start time must be earlier than end time.");
            }
        }

        var duplicates = items
            .GroupBy(x => new
            {
                x.DayOfWeek,
                x.StartTime,
                x.EndTime
            })
            .Any(g => g.Count() > 1);

        if (duplicates)
        {
            throw new InvalidOperationException(
                "Duplicate availability is not allowed.");
        }

        foreach (var group in items.GroupBy(x => x.DayOfWeek))
        {
            var ordered = group
                .OrderBy(x => x.StartTime)
                .ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                var previous = ordered[i - 1];
                var current = ordered[i];

                if (current.StartTime < previous.EndTime)
                {
                    throw new InvalidOperationException(
                        $"Overlapping availability found for {group.Key}.");
                }
            }
        }
    }

    private static TutorApplicationDto ToDto(
        TutorApplication application)
    {
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

        return new TutorApplicationDto(
            application.Id,
            application.Status,
            application.Qualifications,
            application.Introduction,
            subjects,
            availabilities,
            application.AdminNote,
            application.SubmittedAt,
            application.ReviewedAt);
    }
}