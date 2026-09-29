using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduBook.Application.DTOs;
using EduBook.Domain.Entities;
using EduBook.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
            if (req.Role is not (UserRoles.Student or UserRoles.Tutor))
            {
                return BadRequest(new { message = "Role must be Student or Tutor." });
            }

            var existing = await _userManager.FindByEmailAsync(req.Email);
            if (existing != null)
            {
                return Conflict(new { message = "Email already registered." });
            }

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                UserName = req.Email,
                Email = req.Email,
                FullName = req.FullName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Register failed.", errors = result.Errors });
            }

            await _userManager.AddToRoleAsync(user, req.Role);

            var token = CreateToken(user, req.Role);
            return Ok(new AuthResponse(token, user.Id, user.Email!, req.Role, user.FullName));
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
