using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;

namespace ABC.Models
{
    public class ProductEntity : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom fields
        public string? ProductName { get; set; }
        public double? ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductImageBlobName { get; set; } // Updated from ProductImage
        public string? ProductImageUrl { get; set; } // Updated from ImageSasUrl
    }
}