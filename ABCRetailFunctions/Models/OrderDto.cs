using System;
#nullable enable
namespace ABCRetailFunctions.Models
{
    public class OrderDto
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string? ETag { get; set; }

        // Custom fields
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Status { get; set; } // Added
    }
}