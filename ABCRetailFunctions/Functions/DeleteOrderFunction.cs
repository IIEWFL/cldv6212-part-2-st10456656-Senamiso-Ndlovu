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
    public class DeleteOrderFunction
    {
        private readonly OrderQueueService _orderQueue;

        public DeleteOrderFunction(OrderQueueService orderQueue)
        {
            _orderQueue = orderQueue;
        }

        [FunctionName("DeleteOrder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "orders/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to delete order with RowKey: {rowKey}");

            try
            {
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                // Send to queue
                var queueMessage = new OrderQueueMessage
                {
                    Action = "delete",
                    RowKey = rowKey
                };
                await _orderQueue.SendMessageAsync(JsonConvert.SerializeObject(queueMessage));

                log.LogInformation($"Queued delete order for processing.");

                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error queuing order delete: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}