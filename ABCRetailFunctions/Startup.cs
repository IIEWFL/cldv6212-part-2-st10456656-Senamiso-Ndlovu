using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ABCRetailFunctions.Services;
using ABCRetailFunctions.Services.Storage;

[assembly: FunctionsStartup(typeof(ABCRetailFunctions.Startup))]

namespace ABCRetailFunctions
{
    public class Startup : FunctionsStartup
    {
        private BlobStorageService _blobService;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new InvalidOperationException("Storage connection string is not set.");
            }

            // Register Blob Storage
            _blobService = new BlobStorageService(storageConnectionString, "product-photos");
            builder.Services.AddSingleton(_blobService);

            // Register table storage services
            builder.Services.AddSingleton(sp => CreateStorageService<CustomerService>(sp, "Customer", "customertable"));
            builder.Services.AddSingleton(sp => CreateStorageService<ProductService>(sp, "Product", "producttable"));
            builder.Services.AddSingleton(sp => CreateStorageService<OrderService>(sp, "Order", "ordertable"));

            // Register file share for logs
            builder.Services.AddSingleton<FileShareStorageService>(new FileShareStorageService(storageConnectionString, "log-files"));

            // Register queue services
            builder.Services.AddSingleton<AuditQueueService>(new AuditQueueService(storageConnectionString));
            builder.Services.AddSingleton<OrderQueueService>(new OrderQueueService(storageConnectionString));
        }

        private T CreateStorageService<T>(IServiceProvider sp, string serviceIdentifier, string serviceType) where T : class
        {
            var logger = sp.GetRequiredService<ILogger<Startup>>(); // Generic logger for now, adjust as needed
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrWhiteSpace(serviceIdentifier))
            {
                logger.LogError("Storage Connection String or Service Identifier is not set");
                throw new InvalidOperationException("Configuration is invalid");
            }

            logger.LogInformation($"Using {serviceType} identifier: {serviceIdentifier}");

            return serviceType switch
            {
                "customertable" => new CustomerService(storageConnectionString, serviceIdentifier) as T,
                "producttable" => new ProductService(storageConnectionString, serviceIdentifier, _blobService, sp.GetRequiredService<ILogger<ProductService>>()) as T,
                "ordertable" => new OrderService(storageConnectionString, serviceIdentifier, sp.GetRequiredService<ILogger<OrderService>>()) as T,
                _ => throw new NotImplementedException($"{serviceType} is not supported")
            };
        }
    }
}