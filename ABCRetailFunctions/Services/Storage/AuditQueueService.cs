using Azure.Storage.Queues;
using System.Text.Json;
using System.Text;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Services.Storage
{
    public class AuditQueueService : QueueStorageService
    {
        public AuditQueueService(string storageConnectionString) : base(storageConnectionString, "audit-queue") { }
    }
}