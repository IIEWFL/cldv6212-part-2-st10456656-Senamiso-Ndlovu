using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;

namespace ABCRetailFunctions.Functions
{
    public class DeleteCustomerFunction
    {
        private readonly CustomerService _table;
        private readonly AuditQueueService _auditQueue; // Added for audit

        public DeleteCustomerFunction(CustomerService customerService, AuditQueueService auditQueue)
        {
            _table = customerService;
            _auditQueue = auditQueue;
        }

        [FunctionName("DeleteCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to delete a customer with RowKey: {rowKey}");

            try
            {
                // Validate rowKey
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                // Check if customer exists and get name for audit
                var customer = await _table.GetByIdAsync(rowKey);
                if (customer == null)
                {
                    log.LogWarning($"Customer not found with RowKey: {rowKey}");
                    return new NotFoundResult();
                }

                // Delete the customer record
                await _table.DeleteAsync(rowKey);
                log.LogInformation($"Deleted customer with RowKey: {rowKey}");

                // Send audit log (added)
                var auditLog = new AuditLog
                {
                    Action = "delete",
                    Entity = "Customer",
                    Id = rowKey,
                    Name = customer.CustomerName,
                    Timestamp = DateTimeOffset.Now
                };
                await _auditQueue.SendLogEntryAsync(auditLog);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error deleting customer: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}