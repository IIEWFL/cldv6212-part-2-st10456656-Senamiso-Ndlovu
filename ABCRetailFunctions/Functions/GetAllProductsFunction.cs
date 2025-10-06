using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Models;
using System.Linq;

namespace ABCRetailFunctions.Functions
{
    public class GetAllProductsFunction
    {
        private readonly ProductService _productService;

        public GetAllProductsFunction(ProductService productService)
        {
            _productService = productService;
        }

        [FunctionName("GetAllProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to get all products.");

            // Retrieve products from table storage 
            var products = await _productService.GetAllProductsAsync();

            // Convert to DTOs
            var productDtos = products.Select(p => new ProductDto
            {
                PartitionKey = p.PartitionKey,
                RowKey = p.RowKey,
                ProductName = p.ProductName,
                ProductPrice = p.ProductPrice,
                ProductDescription = p.ProductDescription,
                ProductImageUrl = p.ProductImageUrl,
                Timestamp = p.Timestamp,
                ETag = p.ETag.ToString()
            }).ToList();

            return new OkObjectResult(productDtos);
        }
    }
}