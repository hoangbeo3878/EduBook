using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduBook.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var db =
            serviceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedSubjectsAsync(db);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[]
        {
            UserRoles.Student,
            UserRoles.Tutor,
            UserRoles.Admin
        };

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>
                {
                    Id = Guid.CreateVersion7(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to seed role '{roleName}': {errors}");
            }
        }
    }

    private static async Task SeedSubjectsAsync(AppDbContext db)
    {
        var subjects = new[]
        {
            new Subject
            {
                Name = "Toán",
                Description = "Toán phổ thông / luyện thi"
            },
            new Subject
            {
                Name = "Tiếng Anh",
                Description = "Giao tiếp & IELTS"
            },
            new Subject
            {
                Name = "Lập trình",
                Description = "C# / Web cơ bản"
            }
        };

        foreach (var subject in subjects)
        {
            var exists = await db.Subjects
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Name == subject.Name);

            if (exists)
                continue;

            db.Subjects.Add(subject);
        }

        await db.SaveChangesAsync();
    }
}