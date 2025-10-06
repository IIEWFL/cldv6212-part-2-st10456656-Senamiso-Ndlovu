#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Models
{
    public class CustomerDto 
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string? ETag { get; set; }

        // Custom fields
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
