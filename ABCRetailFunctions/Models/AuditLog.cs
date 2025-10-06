using System;

namespace ABCRetailFunctions.Models
{
    public class AuditLog
    {
        public string Action { get; set; }
        public string Entity { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? Timestamp { get; set; } // Use DateTimeOffset? to fix CS0029
        public string MessageId { get; set; }
        public DateTimeOffset? InsertionTime { get; set; }
        public string RawMessage { get; set; }
    }
}