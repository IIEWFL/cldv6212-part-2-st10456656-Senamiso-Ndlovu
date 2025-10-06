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
    public class UpdateCustomerFunction
    {
        private readonly CustomerService _table;
        private readonly AuditQueueService _auditQueue; // Added for audit

        public UpdateCustomerFunction(CustomerService customerService, AuditQueueService auditQueue)
        {
            _table = customerService;
            _auditQueue = auditQueue;
        }

        [FunctionName("UpdateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customers/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to update a customer with RowKey: {rowKey}");

            try
            {
                // Validate rowKey
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                // Check if customer exists
                var existingCustomer = await _table.GetByIdAsync(rowKey);
                if (existingCustomer == null)
                {
                    log.LogWarning($"Customer not found with RowKey: {rowKey}");
                    return new NotFoundResult();
                }

                // Read form data
                var form = await req.ReadFormAsync();
                var customer = new CustomerEntity
                {
                    PartitionKey = "CUSTOMER",
                    RowKey = rowKey,
                    CustomerName = form["CustomerName"],
                    CustomerEmail = form["CustomerEmail"],
                    ETag = existingCustomer.ETag // Preserve ETag for concurrency
                };

                // Validate input
                if (string.IsNullOrWhiteSpace(customer.CustomerName) || string.IsNullOrWhiteSpace(customer.CustomerEmail))
                {
                    log.LogWarning("Invalid customer data provided.");
                    return new BadRequestObjectResult("Customer name and email are required.");
                }

                // Update customer
                await _table.UpdateAsync(customer);
                log.LogInformation($"Updated customer with RowKey: {rowKey}");

                // Send audit log (added)
                var auditLog = new AuditLog
                {
                    Action = "update",
                    Entity = "Customer",
                    Id = rowKey,
                    Name = customer.CustomerName,
                    Timestamp = DateTimeOffset.Now
                };
                await _auditQueue.SendLogEntryAsync(auditLog);

                // Convert to DTO for response
                var customerDto = new CustomerDto
                {
                    PartitionKey = customer.PartitionKey,
                    RowKey = customer.RowKey,
                    CustomerName = customer.CustomerName,
                    CustomerEmail = customer.CustomerEmail,
                    Timestamp = customer.Timestamp,
                    ETag = customer.ETag.ToString()
                };

                return new OkObjectResult(customerDto);
            }
            catch (Exception ex)
            {
                log.LogError($"Error updating customer: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}