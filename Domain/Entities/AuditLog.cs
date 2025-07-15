namespace Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string Action { get; set; } = null!;
        public string? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
