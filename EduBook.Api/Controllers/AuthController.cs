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

namespace EduBook.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }
        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var fullName = req.FullName.Trim();

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                UserName = email,
                Email = email,
                FullName = fullName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var existing = await _userManager.FindByEmailAsync(req.Email);
            if (existing != null)
            {
                return Conflict(new 
                {
                    message = "Email already registered." 
                });
            }   

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

            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Student);

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

            var roles = await _userManager.GetRolesAsync(user);

            var token = CreateToken(user, UserRoles.Student);

            return Ok(new AuthResponse(token, user.Id, user.Email!, UserRoles.Student, user.FullName));
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var ok = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!ok)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? UserRoles.Student;

            var token = CreateToken(user, role);
            return Ok(new AuthResponse(token, user.Id, user.Email!, role, user.FullName));
        }

        // GET /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<MeResponse>> Me()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(raw, out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? UserRoles.Student;

            return Ok(new MeResponse(user.Id, user.Email!, role, user.FullName));
        }

        private string CreateToken(User user, string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var minutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
