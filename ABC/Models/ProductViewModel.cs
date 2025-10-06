namespace ABC.Models
{
    public class ProductViewModel
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string? ProductName { get; set; }
        public double? ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public string? ProductImage { get; set; } // Matches ProductEntity
        public string? ImageSasUrl { get; set; } // For display purposes
    }
}