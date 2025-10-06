#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;

namespace ABCRetailFunctions.Functions
{
    public class CreateProductFunction
    {
        private readonly ProductService _productService;
        private readonly AuditQueueService _auditQueue;

        public CreateProductFunction(ProductService productService, AuditQueueService auditQueue)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _auditQueue = auditQueue ?? throw new ArgumentNullException(nameof(auditQueue));
        }

        [FunctionName("CreateProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to create a new product.");

            try
            {
                var form = await req.ReadFormAsync();
                var product = new ProductEntity
                {
                    PartitionKey = "PRODUCT",
                    RowKey = Guid.NewGuid().ToString(),
                    ProductName = form["ProductName"],
                    ProductPrice = double.TryParse(form["ProductPrice"], out var price) ? price : null,
                    ProductDescription = form["ProductDescription"]
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

                var success = await _productService.AddAsync(product, imageStream);
                if (!success)
                {
                    log.LogWarning("Failed to create product in storage.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                log.LogInformation($"Created product with PartitionKey: {product.PartitionKey}, RowKey: {product.RowKey}");

                var auditLog = new AuditLog
                {
                    Action = "create",
                    Entity = "Product",
                    Id = product.RowKey,
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

                return new CreatedResult($"/api/products/{product.RowKey}", productDto);
            }
            catch (Exception ex)
            {
                log.LogError($"Error creating product: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}