using System;
using System.IO;
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
    public class CreateOrderFunction
    {
        private readonly OrderQueueService _orderQueue;

        public CreateOrderFunction(OrderQueueService orderQueue)
        {
            _orderQueue = orderQueue;
        }

        [FunctionName("CreateOrder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to create a new order.");

            try
            {
                var form = await req.ReadFormAsync();
                var order = new OrderEntity
                {
                    CustomerId = form["CustomerId"],
                    ProductId = form["ProductId"],
                    Status = form["Status"],// Added, allow override but default Pending
                    Quantity = int.TryParse(form["Quantity"], out var qty) ? qty : 0,
                    OrderDate = DateTimeOffset.Now,
                    TotalPrice = double.TryParse(form["TotalPrice"], out var price) ? price : 0

                };

                // Validate input
                if (string.IsNullOrWhiteSpace(order.CustomerId) || string.IsNullOrWhiteSpace(order.ProductId) || order.Quantity <= 0 || order.TotalPrice <= 0)
                {
                    log.LogWarning("Invalid order data provided.");
                    return new BadRequestObjectResult("Customer ID, Product ID, Quantity, and Total Price are required.");
                }

                // Send to queue instead of direct to table
                var queueMessage = new OrderQueueMessage
                {
                    Action = "create",
                    Data = order
                };
                await _orderQueue.SendMessageAsync(JsonConvert.SerializeObject(queueMessage));

                log.LogInformation($"Queued create order for processing.");

                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error queuing order create: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}