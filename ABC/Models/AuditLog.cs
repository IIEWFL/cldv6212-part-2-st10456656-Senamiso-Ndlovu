namespace ABC.Models
{
    public class AuditLog
    {
        public string? MessageId { get; set; }
        public DateTimeOffset? InsertionTime { get; set; }

        // From the JSON payload
        public string? Action { get; set; }
        public string? Entity { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Timestamp { get; set; }

        // Fallback
        public string? RawMessage { get; set; }
    }
}
