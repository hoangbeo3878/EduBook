using EduBook.Infrastructure;
using EduBook.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

// Đăng ký toàn bộ dịch vụ Infrastructure, DB, Identity & Auth
builder.Services.AddInfrastructureServices(builder.Configuration);

// Đăng ký Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Tạo các Roles mặc định (Student, Tutor)
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();