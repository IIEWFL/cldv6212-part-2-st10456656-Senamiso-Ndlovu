using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs; // Added to fix CS0246
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;

namespace ABCRetailFunctions.Functions
{
    public class ProcessOrderQueueFunction
    {
        private readonly OrderService _orderService;
        private readonly AuditQueueService _auditQueue;

        public ProcessOrderQueueFunction(OrderService orderService, AuditQueueService auditQueue)
        {
            _orderService = orderService;
            _auditQueue = auditQueue;
        }

        [FunctionName("ProcessOrderQueue")]
        public async Task Run([QueueTrigger("order-queue", Connection = "storageConnectionString")] string message, ILogger log)
        {
            log.LogInformation($"Processing order queue message: {message}");

            try
            {
                var queueMessage = JsonConvert.DeserializeObject<OrderQueueMessage>(message);
                if (queueMessage == null)
                {
                    log.LogWarning("Invalid queue message.");
                    return;
                }

                var auditLog = new AuditLog
                {
                    Entity = "Order",
                    Timestamp = DateTimeOffset.Now
                };

                switch (queueMessage.Action.ToLower())
                {
                    case "create":
                        var createEntity = queueMessage.Data;
                        if (createEntity == null) return;
                        await _orderService.AddAsync(createEntity);
                        auditLog.Action = "create";
                        auditLog.Id = createEntity.RowKey;
                        auditLog.Name = $"Order for customer {createEntity.CustomerId}";
                        createEntity.Status ??= "Pending"; // Ensure default
                        break;

                    case "update":
                        var updateEntity = queueMessage.Data;
                        if (updateEntity == null || string.IsNullOrEmpty(updateEntity.RowKey)) return;
                        await _orderService.UpdateAsync(updateEntity);
                        auditLog.Action = "update";
                        auditLog.Id = updateEntity.RowKey;
                        auditLog.Name = $"Order for customer {updateEntity.CustomerId}";
                        break;

                    case "delete":
                        if (string.IsNullOrEmpty(queueMessage.RowKey)) return;
                        await _orderService.DeleteAsync(queueMessage.RowKey);
                        auditLog.Action = "delete";
                        auditLog.Id = queueMessage.RowKey;
                        auditLog.Name = "Deleted Order";
                        break;

                    default:
                        log.LogWarning($"Unknown action: {queueMessage.Action}");
                        return;
                }

                // Send audit log
                await _auditQueue.SendLogEntryAsync(auditLog);
                log.LogInformation($"Processed {queueMessage.Action} for order {queueMessage.RowKey ?? queueMessage.Data?.RowKey}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing order queue: {ex.Message}");
            }
        }
    }
}