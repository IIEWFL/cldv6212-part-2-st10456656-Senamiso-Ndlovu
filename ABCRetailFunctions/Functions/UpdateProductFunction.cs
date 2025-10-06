#nullable enable
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Functions
{
    public class UpdateProductFunction
    {
        private readonly ProductService _productService;
        private readonly AuditQueueService _auditQueue;

        public UpdateProductFunction(ProductService productService, AuditQueueService auditQueue)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _auditQueue = auditQueue ?? throw new ArgumentNullException(nameof(auditQueue));
        }

        [FunctionName("UpdateProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to update product with RowKey: {rowKey}");

            try
            {
                if (string.IsNullOrWhiteSpace(rowKey))
                {
                    log.LogWarning("RowKey is missing.");
                    return new BadRequestObjectResult("RowKey is required.");
                }

                var existingProduct = await _productService.GetByIdAsync(rowKey);
                if (existingProduct == null)
                {
                    log.LogWarning($"Product not found with RowKey: {rowKey}");
                    return new NotFoundResult();
                }

                var form = await req.ReadFormAsync();
                var product = new ProductEntity
                {
                    PartitionKey = "PRODUCT",
                    RowKey = rowKey,
                    ProductName = form["ProductName"],
                    ProductPrice = double.TryParse(form["ProductPrice"], out var price) ? price : null,
                    ProductDescription = form["ProductDescription"],
                    ETag = existingProduct.ETag // Preserve ETag for concurrency
                };

                if (string.IsNullOrWhiteSpace(product.ProductName) || product.ProductPrice == null)
                {
                    log.LogWarning("Invalid product data provided.");
                    return new BadRequestObjectResult("Product name and price are required.");
                }

                IFormFile? imageFile = req.Form.Files["imageFile"];
                Stream? imageStream = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    imageStream = imageFile.OpenReadStream();
                }

                var success = await _productService.UpdateAsync(product, imageStream);
                if (!success)
                {
                    log.LogWarning($"Failed to update product with RowKey: {rowKey}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                log.LogInformation($"Updated product with RowKey: {rowKey}");

                var auditLog = new AuditLog
                {
                    Action = "update",
                    Entity = "Product",
                    Id = rowKey,
                    Name = product.ProductName,
                    Timestamp = DateTimeOffset.Now
                };
                await _auditQueue.SendLogEntryAsync(auditLog);

                var productDto = new ProductDto
                {
                    PartitionKey = product.PartitionKey,
                    RowKey = product.RowKey,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    ProductDescription = product.ProductDescription,
                    ProductImageUrl = product.ProductImageUrl,
                    Timestamp = product.Timestamp,
                    ETag = product.ETag.ToString()
                };

                return new OkObjectResult(productDto);
            }
            catch (Exception ex)
            {
                log.LogError($"Error updating product: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}