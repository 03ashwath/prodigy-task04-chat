using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public int? RoomId { get; set; }
        public ChatRoom? Room { get; set; }

        public int? ReceiverId { get; set; }
        public User? Receiver { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string MessageType { get; set; } = "text";

        public string? FileUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}