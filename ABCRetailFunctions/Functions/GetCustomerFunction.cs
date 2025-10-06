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

namespace ABCRetailFunctions.Functions
{
    public  class GetCustomerFunction
    {
        private CustomerService _table;

        // Constructor with connection string + table name
        public GetCustomerFunction(CustomerService customerservice)
        {
            _table = customerservice;
        }


        [FunctionName("GetCustomer")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{rowkey}")] HttpRequest req, string rowKey,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to get customer details based on rowkey: {rowKey}");


            //retrieve student from the table storage
            var customer = await _table.GetByIdAsync(rowKey);
            if(customer == null)
            {
                log.LogWarning($"Student not found on rowkey:{rowKey}");
                return new NotFoundResult();
            }

            //Convert to Customer to customerDto
            var customerDto = new CustomerDto
            {
                PartitionKey = customer.PartitionKey,
                RowKey = customer.RowKey,
                Timestamp = customer.Timestamp,
                CustomerName = customer.CustomerName,
                CustomerEmail = customer.CustomerEmail,
                ETag = customer.ETag.ToString(),
            };

            return new OkObjectResult(customerDto);
        }
    }
}
