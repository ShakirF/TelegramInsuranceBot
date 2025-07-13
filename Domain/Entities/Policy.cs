namespace Domain.Entities
{
    public class Policy
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FilePath { get; set; } = default!;
        public string DocumentSummary { get; set; } = null!;
        public string GptMessage { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiryAt { get; set; }
        public bool IsSentToUser { get; set; } = false;

        public User User { get; set; } = default!;
    }
}
