using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.ChatRooms
                .Where(r => !r.IsPrivate)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.CreatedAt,
                    r.IsPrivate,
                    MemberCount = r.Members.Count,
                    LastMessage = r.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => new { m.Content, m.SentAt })
                        .FirstOrDefault()
                }).ToListAsync();
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var room = new ChatRoom
            {
                Name = dto.Name,
                Description = dto.Description,
                IsPrivate = false,
                CreatedByUserId = userId
            };
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.RoomId == id)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * 50)
                .Take(50)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    m.MessageType,
                    m.FileUrl,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    SenderAvatar = m.Sender.Avatar
                })
                .ToListAsync();

            return Ok(messages.OrderBy(m => m.SentAt));
        }

        [HttpGet("private/{userId}")]
        public async Task<IActionResult> GetPrivateMessages(int userId)
        {
            var myId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m =>
                    (m.SenderId == myId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.SentAt,
                    m.MessageType,
                    m.IsRead,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    SenderAvatar = m.Sender.Avatar
                })
                .ToListAsync();

            return Ok(messages);
        }
    }

    public class CreateRoomDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}