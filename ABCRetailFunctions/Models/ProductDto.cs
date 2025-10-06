#nullable enable
using System;

namespace ABCRetailFunctions.Models
{
    public class ProductDto
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string? ETag { get; set; }

        // Custom fields
        public string? ProductName { get; set; }
        public double? ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}