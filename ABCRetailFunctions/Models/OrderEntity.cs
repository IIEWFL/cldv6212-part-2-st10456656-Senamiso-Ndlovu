using Azure;
#nullable enable
using Azure.Data.Tables;
using System;

namespace ABCRetailFunctions.Models
{
    public class OrderEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom fields
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Status { get; set; } = "Pending"; // Added
    }
}