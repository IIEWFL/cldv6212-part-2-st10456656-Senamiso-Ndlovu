using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ABCRetailFunctions.Services.Storage;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Functions
{
    public class UpdateOrderFunction
    {
        private readonly OrderQueueService _orderQueue;

        public UpdateOrderFunction(OrderQueueService orderQueue)
        {
            _orderQueue = orderQueue;
        }

        [FunctionName("UpdateOrder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "orders/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to update order with RowKey: {rowKey}");

            try
            {
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                var form = await req.ReadFormAsync();
                var order = new OrderEntity
                {
                    RowKey = rowKey,
                    CustomerId = form["CustomerId"],
                    ProductId = form["ProductId"],
                    Quantity = int.TryParse(form["Quantity"], out var qty) ? qty : 0,
                    OrderDate = DateTimeOffset.TryParse(form["OrderDate"], out var date) ? date : DateTimeOffset.Now,
                    Status = form["Status"], 
                    TotalPrice = double.TryParse(form["TotalPrice"], out var price) ? price : 0
                };

                // Validate input
                if (string.IsNullOrWhiteSpace(order.CustomerId) || string.IsNullOrWhiteSpace(order.ProductId) || order.Quantity <= 0 || order.TotalPrice <= 0)
                {
                    log.LogWarning("Invalid order data provided.");
                    return new BadRequestObjectResult("Customer ID, Product ID, Quantity, and Total Price are required.");
                }

                // Send to queue
                var queueMessage = new OrderQueueMessage
                {
                    Action = "update",
                    Data = order
                };
                await _orderQueue.SendMessageAsync(JsonConvert.SerializeObject(queueMessage));

                log.LogInformation($"Queued update order for processing.");

                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error queuing order update: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}