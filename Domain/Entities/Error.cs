namespace Domain.Entities
{
    public class Error
    {
        public int Id { get; set; }
        public long? TelegramUserId { get; set; }
        public string Message { get; set; } = null!;
        public string? StackTrace { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
