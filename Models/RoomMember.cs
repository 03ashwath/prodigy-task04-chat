namespace ChatApp.Models
{
    public class RoomMember
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public ChatRoom Room { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Role { get; set; } = "member";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}