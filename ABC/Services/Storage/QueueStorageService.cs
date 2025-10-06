using ABC.Models;
using System.Text.Json;
using System.Text;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABC.Services.Storage
{
    public class QueueStorageService
    {
        
            //defined queue client
            private readonly QueueClient _queueClient;

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

            //Get log entries from queue
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

        
    }
}
