using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using ABCRetailFunctions.Services.Storage;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Functions
{
    public class GenerateLogFileFunction
    {
        private readonly AuditQueueService _auditQueue;
        private readonly FileShareStorageService _fileService;

        public GenerateLogFileFunction(AuditQueueService auditQueue, FileShareStorageService fileService)
        {
            _auditQueue = auditQueue;
            _fileService = fileService;
        }

        [FunctionName("GenerateLogFile")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log) // Every 5 minutes
        {
            log.LogInformation($"Generating log file at: {DateTime.Now}");

            var messages = await _auditQueue.ReceiveMessagesAsync(32);
            if (messages.Count == 0) return;

            var auditLogs = new List<AuditLog>();

            foreach (var msg in messages)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.Body.ToString()));
                    var deserialized = JsonSerializer.Deserialize<AuditLog>(json);
                    if (deserialized != null)
                    {
                        deserialized.MessageId = msg.MessageId;
                        deserialized.InsertionTime = msg.InsertedOn;
                        auditLogs.Add(deserialized);
                    }
                }
                catch (Exception ex)
                {
                    log.LogWarning($"Failed to deserialize message {msg.MessageId}: {ex.Message}");
                }

                // Delete message after processing
                await _auditQueue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            }

            if (auditLogs.Count > 0)
            {
                string fileName = $"audit-log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";
                await _fileService.UploadAuditLogAsync(auditLogs, fileName);
                log.LogInformation($"Uploaded {auditLogs.Count} logs to {fileName}");
            }
        }
    }
}