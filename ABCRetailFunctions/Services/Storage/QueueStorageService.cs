using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ABCRetailFunctions.Models;

namespace ABCRetailFunctions.Services.Storage
{
    public class QueueStorageService
    {

        //defined queue client
        protected readonly QueueClient _queueClient;

        //initialise the constructor
        public QueueStorageService(string storageConnectionString, string queueName)
        {
            var queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
        }

        //Send Log entry to queue
        public async Task SendLogEntryAsync(object message)
        {
            //Convert the message to a JSON string
            var jsonMessage = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonMessage)));
        }

        // Send a raw message to queue (for order queue)
        public async Task SendMessageAsync(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        //Get log entries from queue (peek)
        public async Task<List<AuditLog>> GetLogEntriesAsync()
        {
            var entryList = new List<AuditLog>();
            var entries = await _queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (PeekedMessage entry in entries.Value)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(entry.Body.ToString()));

                    var deserialized = JsonSerializer.Deserialize<AuditLog>(json);

                    if (deserialized != null)
                    {
                        deserialized.MessageId = entry.MessageId;
                        deserialized.InsertionTime = entry.InsertedOn;
                        entryList.Add(deserialized);
                    }
                }
                catch
                {
                    entryList.Add(new AuditLog
                    {
                        MessageId = entry.MessageId,
                        InsertionTime = entry.InsertedOn,
                        RawMessage = entry.Body.ToString()
                    });
                }
            }
            return entryList;
        }

        // Receive messages (dequeue)
        public async Task<List<QueueMessage>> ReceiveMessagesAsync(int maxMessages = 32)
        {
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages);
            return response.Value.ToList();
        }

        // Delete a message
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}