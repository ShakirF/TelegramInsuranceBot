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
        public int UploadRetryCount { get; set; } = 0;

        public ICollection<UserState> States { get; set; } = new List<UserState>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}
