using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private static Dictionary<string, int> _connections = new();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _connections[Context.ConnectionId] = userId;

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = true;
                user.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await Clients.All.SendAsync("UserOnline", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var userId))
            {
                _connections.Remove(Context.ConnectionId);

                if (!_connections.Values.Contains(userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.IsOnline = false;
                        user.LastSeen = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    await Clients.All.SendAsync("UserOffline", userId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
            var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            await Clients.Group($"room_{roomId}").SendAsync("UserJoinedRoom", new { userId, userName = user?.Name, roomId });
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }

        public async Task SendRoomMessage(int roomId, string content)
        {
            var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            var message = new Message
            {
                SenderId = userId,
                RoomId = roomId,
                Content = content,
                MessageType = "text",
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Group($"room_{roomId}").SendAsync("ReceiveRoomMessage", new
            {
                message.Id,
                message.Content,
                message.SentAt,
                message.MessageType,
                SenderId = userId,
                SenderName = user?.Name,
                SenderAvatar = user?.Avatar,
                RoomId = roomId
            });
        }

        public async Task SendPrivateMessage(int receiverId, string content)
        {
            var senderId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var sender = await _context.Users.FindAsync(senderId);

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                MessageType = "text",
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var msgData = new
            {
                message.Id,
                message.Content,
                message.SentAt,
                message.MessageType,
                SenderId = senderId,
                SenderName = sender?.Name,
                SenderAvatar = sender?.Avatar,
                ReceiverId = receiverId
            };

            var receiverConnections = _connections
                .Where(c => c.Value == receiverId)
                .Select(c => c.Key).ToList();

            foreach (var conn in receiverConnections)
                await Clients.Client(conn).SendAsync("ReceivePrivateMessage", msgData);

            await Clients.Caller.SendAsync("ReceivePrivateMessage", msgData);
        }

        public async Task SendTyping(int roomId, bool isTyping)
        {
            var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserTyping", new { userId, userName = user?.Name, isTyping, roomId });
        }
    }
}