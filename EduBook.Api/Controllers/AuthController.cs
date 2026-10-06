using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduBook.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<User> userManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var fullName = req.FullName.Trim();

        var existing = await _userManager.FindByEmailAsync(email);

        if (existing != null)
        {
            return Conflict(new
            {
                message = "Email already registered."
            });
        }

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            FullName = fullName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(
            user,
            req.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Register failed.",
                errors = result.Errors
            });
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            UserRoles.Student);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(e => e.Description));

            return BadRequest(new
            {
                message = "Failed to assign default role.",
                errors
            });
        }

        var roles = (await _userManager.GetRolesAsync(user))
            .ToList();

        var token = CreateToken(user, roles);

        return Ok(
            new AuthResponse(
                token,
                user.Id,
                user.Email!,
                roles,
                user.FullName));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var ok = await _userManager.CheckPasswordAsync(
            user,
            req.Password);

        if (!ok)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var roles = (await _userManager.GetRolesAsync(user))
            .ToList();

        // Fallback for users that somehow have no role.
        if (roles.Count == 0)
        {
            roles.Add(UserRoles.Student);
        }

        var token = CreateToken(user, roles);

        return Ok(
            new AuthResponse(
                token,
                user.Id,
                user.Email!,
                roles,
                user.FullName));
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var raw = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(raw, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user == null)
        {
            return Unauthorized();
        }

        var roles = (await _userManager.GetRolesAsync(user))
            .ToList();

        return Ok(new
        {
            user.Id,
            user.Email,
            Roles = roles,
            user.FullName
        });
    }

    // Create JWT token
    private string CreateToken(
        User user,
        IEnumerable<string> roles)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Missing Jwt:Key configuration.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Email,
                user.Email ?? string.Empty)
        };

        // Add every role as a separate Role claim.
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var expireMinutes =
            _config.GetValue<int>("Jwt:ExpireMinutes");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}