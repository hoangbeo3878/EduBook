using EduBook.Application.Abstractions.Services;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using EduBook.Infrastructure.Data;
using EduBook.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace EduBook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cấu hình phản hồi lỗi khi ModelState không hợp lệ
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? "Invalid value."
                                : e.ErrorMessage)
                            .ToArray());

                return new BadRequestObjectResult(new
                {
                    message = "Validation failed.",
                    errors
                });
            };
        });
        // Cấu hình Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Đăng ký Application Services (DI)
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITutorApplicationService, TutorApplicationService>();

        // Cấu hình ASP.NET Core Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Cấu hình Xác thực JWT Authentication
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Missing Jwt:Key");
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
            };
        });

        services.AddAuthorization();

        return services;
    }
}