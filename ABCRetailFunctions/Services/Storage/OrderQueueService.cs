using Azure.Storage.Queues;
using System.Text.Json;
using System.Text;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Services.Storage
{
    public class OrderQueueService : QueueStorageService
    {
        public OrderQueueService(string storageConnectionString) : base(storageConnectionString, "order-queue") { }
    }
}