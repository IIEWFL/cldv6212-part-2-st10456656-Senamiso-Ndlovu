using Azure;
using Azure.Data.Tables;

namespace ABC.Models
{
    public class CustomerEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom fields
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
