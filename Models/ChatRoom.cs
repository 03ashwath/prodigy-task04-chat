using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = false;

        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
    }
}