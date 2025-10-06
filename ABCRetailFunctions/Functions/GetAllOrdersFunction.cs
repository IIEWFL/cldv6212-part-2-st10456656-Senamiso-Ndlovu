using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;
using System.Linq;

namespace ABCRetailFunctions.Functions
{
    public class GetAllOrdersFunction
    {
        private readonly OrderService _orderService;

        public GetAllOrdersFunction(OrderService orderService)
        {
            _orderService = orderService;
        }

        [FunctionName("GetAllOrders")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to get all orders.");

            var orders = await _orderService.GetAllOrdersAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                PartitionKey = o.PartitionKey,
                RowKey = o.RowKey,
                CustomerId = o.CustomerId,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                Timestamp = o.Timestamp,
                ETag = o.ETag.ToString()
            }).ToList();

            return new OkObjectResult(orderDtos);
        }
    }
}