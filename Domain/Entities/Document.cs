using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public string FileId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string LocalPath { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? OcrRawJson { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ContentHash { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<ExtractedField> ExtractedFields { get; set; } = new List<ExtractedField>();
    }
}
