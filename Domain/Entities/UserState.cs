namespace Domain.Entities
{
    public class UserState
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string CurrentStep { get; set; } = "start";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
