using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Functions
{
    public class GetProductFunction
    {
        private readonly ProductService _productService;

        public GetProductFunction(ProductService productService)
        {
            _productService = productService;
        }

        [FunctionName("GetProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{rowKey}")] HttpRequest req,
            string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get product with RowKey: {rowKey}");

            var product = await _productService.GetByIdAsync(rowKey);
            if (product == null)
            {
                log.LogWarning($"Product not found with RowKey: {rowKey}");
                return new NotFoundResult();
            }

            // Convert to DTO
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
    }
}