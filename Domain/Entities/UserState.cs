using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserState
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        [Column(TypeName = "varchar(30)")]
        public UserStep CurrentStep { get; set; } = UserStep.Start;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int CancelRetryCount { get; set; } = 0;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
