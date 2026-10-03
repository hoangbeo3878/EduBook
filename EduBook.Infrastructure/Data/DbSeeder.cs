using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EduBook.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
    IServiceProvider serviceProvider,
    IConfiguration configuration)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<User>>();

        var db =
            serviceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration);
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

    private static async Task SeedAdminAsync(
    UserManager<User> userManager,
    IConfiguration configuration)
    {
        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException(
                "Missing Admin:Email configuration.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Missing Admin:Password configuration.");

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            if (!await userManager.IsInRoleAsync(
                    existingUser,
                    UserRoles.Admin))
            {
                var adminRoleResult = await userManager.AddToRoleAsync(
                    existingUser,
                    UserRoles.Admin);

                if (!adminRoleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        adminRoleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Admin role: {errors}");
                }
            }

            return;
        }

        var admin = new User
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            FullName = "EduBook Administrator",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createResult =
            await userManager.CreateAsync(admin, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to create Admin user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin,
            UserRoles.Admin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to assign Admin role: {errors}");
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