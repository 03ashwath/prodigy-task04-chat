using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        public bool IsOnline { get; set; } = false;

        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
    }
}