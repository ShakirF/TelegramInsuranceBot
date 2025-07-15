namespace Domain.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string Prompt { get; set; } = null!;
        public string Response { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
