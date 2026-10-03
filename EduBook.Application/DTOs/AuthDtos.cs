namespace EduBook.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string Role  // "Student" hoặc "Tutor"
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    string Role,
    string? FullName
);

public record MeResponse(
    Guid UserId,
    string Email,
    string Role,
    string? FullName
);