using ChatApp.Data;
using ChatApp.Helpers;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwt;

        public AuthController(AppDbContext context, JwtHelper jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already registered" });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Avatar = dto.Name.Substring(0, 1).ToUpper()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Name, user.Email, user.Avatar, token = _jwt.GenerateToken(user) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { user.Id, user.Name, user.Email, user.Avatar, token = _jwt.GenerateToken(user) });
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var users = await _context.Users
                .Where(u => u.Id != userId)
                .Select(u => new { u.Id, u.Name, u.Email, u.Avatar, u.IsOnline, u.LastSeen })
                .ToListAsync();
            return Ok(users);
        }
    }

    public class AuthDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}