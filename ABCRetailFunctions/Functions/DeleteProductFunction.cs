using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Functions
{
    public class DeleteProductFunction
    {
        private readonly ProductService _productService;
        private readonly AuditQueueService _auditQueue;

        public DeleteProductFunction(ProductService productService, AuditQueueService auditQueue)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _auditQueue = auditQueue ?? throw new ArgumentNullException(nameof(auditQueue));
        }

        [FunctionName("DeleteProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to delete a product with RowKey: {rowKey}");

            try
            {
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                var product = await _productService.GetByIdAsync(rowKey);
                if (product == null)
                {
                    log.LogWarning($"Product not found with RowKey: {rowKey}");
                    return new NotFoundResult();
                }

                var success = await _productService.DeleteAsync(rowKey);
                if (!success)
                {
                    log.LogWarning($"Failed to delete product with RowKey: {rowKey}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                log.LogInformation($"Deleted product with RowKey: {rowKey}");

                var auditLog = new AuditLog
                {
                    Action = "delete",
                    Entity = "Product",
                    Id = rowKey,
                    Name = product.ProductName,
                    Timestamp = DateTimeOffset.Now
                };
                await _auditQueue.SendLogEntryAsync(auditLog);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error deleting product: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}