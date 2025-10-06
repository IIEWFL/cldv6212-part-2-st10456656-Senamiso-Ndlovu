using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Functions
{
    public class GetOrderFunction
    {
        private readonly OrderService _orderService;

        public GetOrderFunction(OrderService orderService)
        {
            _orderService = orderService;
        }

        [FunctionName("GetOrder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get order with RowKey: {rowKey}");

            var order = await _orderService.GetByIdAsync(rowKey);
            if (order == null)
            {
                log.LogWarning($"Order not found with RowKey: {rowKey}");
                return new NotFoundResult();
            }

            var orderDto = new OrderDto
            {
                PartitionKey = order.PartitionKey,
                RowKey = order.RowKey,
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Timestamp = order.Timestamp,
                ETag = order.ETag.ToString(),
                Status = order.Status // Added
            };

            return new OkObjectResult(orderDto);
        }
    }
}