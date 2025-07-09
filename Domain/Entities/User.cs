namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserState> States { get; set; } = new List<UserState>();
    }
}
