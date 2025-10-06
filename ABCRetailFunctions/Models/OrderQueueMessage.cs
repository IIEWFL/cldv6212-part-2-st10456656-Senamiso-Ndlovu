#nullable enable
namespace ABCRetailFunctions.Models
{
    public class OrderQueueMessage
    {
        public string? Action { get; set; } // create, update, delete
        public OrderEntity? Data { get; set; }
        public string? RowKey { get; set; }
    }
}