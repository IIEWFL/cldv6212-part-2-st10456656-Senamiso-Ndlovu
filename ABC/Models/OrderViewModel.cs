#nullable enable
namespace ABC.Models
{
    public class OrderViewModel
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Status { get; set; } = "Pending"; // Added
    }
}