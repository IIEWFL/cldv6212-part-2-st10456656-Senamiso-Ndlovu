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
    public class CreateCustomerFunction
    {
        private readonly CustomerService _table;
        private readonly AuditQueueService _auditQueue; // Added for audit

        public CreateCustomerFunction(CustomerService customerService, AuditQueueService auditQueue)
        {
            _table = customerService;
            _auditQueue = auditQueue;
        }

        [FunctionName("CreateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to create a new customer.");

            try
            {
                var form = await req.ReadFormAsync();
                var customer = new CustomerEntity
                {
                    PartitionKey = "CUSTOMER",
                    RowKey = Guid.NewGuid().ToString(),
                    CustomerEmail = form["CustomerEmail"],
                    CustomerName = form["CustomerName"]
                };

                // Validate input
                if (string.IsNullOrWhiteSpace(customer.CustomerName) || string.IsNullOrWhiteSpace(customer.CustomerEmail))
                {
                    log.LogWarning("Invalid customer data provided.");
                    return new BadRequestObjectResult("Customer name and email are required.");
                }

                await _table.AddAsync(customer);

                log.LogInformation($"Created customer with PartitionKey: {customer.PartitionKey}, RowKey: {customer.RowKey}");

                // Send audit log (added)
                var auditLog = new AuditLog
                {
                    Action = "create",
                    Entity = "Customer",
                    Id = customer.RowKey,
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

                return new CreatedResult($"/api/customers/{customer.RowKey}", customerDto);
            }
            catch (Exception ex)
            {
                log.LogError($"Error creating customer: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}