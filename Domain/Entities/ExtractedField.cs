namespace Domain.Entities
{
    public class ExtractedField
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;

        public string FieldName { get; set; } = null!;
        public string FieldValue { get; set; } = null!;
    }
}
