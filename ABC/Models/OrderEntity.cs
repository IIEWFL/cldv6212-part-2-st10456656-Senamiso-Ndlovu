#nullable enable
using System;
using Azure;
using Azure.Data.Tables;

namespace ABC.Models
{
    public class OrderEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Status { get; set; } = "Pending"; // Added: Pending, Shipped, Delivered, etc.
    }
}