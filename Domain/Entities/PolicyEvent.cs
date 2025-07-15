namespace Domain.Entities
{
    public class PolicyEvent
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string EventType { get; set; } = null!; 
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
