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
using System.Linq;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Functions
{
    public  class GetAllCustomersFunction
    {
        private CustomerService _table;

        // Constructor with connection string + table name
        public GetAllCustomersFunction(CustomerService customerservice)
        {
            _table = customerservice;
        }


        [FunctionName("GetAllCustomers")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to get customers.");

            // retrieve the customers from the table storage 
            var customers = await _table.GetAllCustomersAsync();

            //convert customer to customer data transfer object
            var customerDtos = customers.Select(c => new CustomerDto
            {
                PartitionKey = c.PartitionKey,
                RowKey = c.RowKey,
                CustomerName = c.CustomerName,
                CustomerEmail = c.CustomerEmail,
                Timestamp = c.Timestamp,
                ETag = c.ETag.ToString(),
            }).ToList();

            //return the list if students as an API response 
            return new OkObjectResult(customerDtos);
        }
    }
}
